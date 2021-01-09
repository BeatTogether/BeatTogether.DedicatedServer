﻿using System;
using System.Net;
using System.Threading;
using BeatTogether.DedicatedServer.Kernel.Abstractions;
using NetCoreServer;
using Serilog;

namespace BeatTogether.DedicatedServer.Kernel.Implementations
{
    public class RelayServer : UdpServer
    {
        private readonly IDedicatedServerPortAllocator _dedicatedServerPortAllocator;
        private readonly ILogger _logger;

        private IPEndPoint _sourceEndPoint;
        private IPEndPoint _targetEndPoint;
        private int _inactivityTimeout;
        private CancellationTokenSource _cancellationTokenSource;

        public RelayServer(
            IDedicatedServerPortAllocator dedicatedServerPortAllocator,
            IPEndPoint endPoint,
            IPEndPoint sourceEndPoint,
            IPEndPoint targetEndPoint,
            int inactivityTimeout = 60)
            : base(endPoint)
        {
            _dedicatedServerPortAllocator = dedicatedServerPortAllocator;
            _logger = Log.ForContext<RelayServer>();

            _sourceEndPoint = sourceEndPoint;
            _targetEndPoint = targetEndPoint;
            _inactivityTimeout = inactivityTimeout;
        }

        #region Protected Methods

        protected override void OnStarted()
        {
            _logger.Information(
                "Starting relay server " +
                $"(EndPoint='{Endpoint}', " +
                $"SourceEndPoint='{_sourceEndPoint}', " +
                $"TargetEndPoint='{_targetEndPoint}')."
            );
            WaitForInactivityTimeout();
            ReceiveAsync();
        }

        protected override void OnStopped()
        {
            _logger.Information(
                "Stopping relay server " +
                $"(EndPoint='{Endpoint}', " +
                $"SourceEndPoint='{_sourceEndPoint}', " +
                $"TargetEndPoint='{_targetEndPoint}')."
            );
            _dedicatedServerPortAllocator.ReleaseRelayServerPort(Endpoint.Port);
        }

        protected override void OnReceived(EndPoint endPoint, ReadOnlySpan<byte> buffer)
        {
            bool queued = false;
            try
            {
                _logger.Verbose($"Handling OnReceived (EndPoint='{endPoint}', Size={buffer.Length}).");
                if (endPoint.Equals(_targetEndPoint))
                {
                    _logger.Verbose(
                        "Routing message from " +
                        $"'{endPoint}' -> '{_sourceEndPoint}' " +
                        $"(Data='{BitConverter.ToString(buffer.ToArray())}')."
                    );
                    queued = SendAsync(_sourceEndPoint, buffer);
                }
                else if (endPoint.Equals(_sourceEndPoint))
                {
                    _logger.Verbose(
                        "Routing message from " +
                        $"'{endPoint}' -> '{_targetEndPoint}' " +
                        $"(Data='{BitConverter.ToString(buffer.ToArray())}')."
                    );
                    queued = SendAsync(_targetEndPoint, buffer);
                }
                else
                {
                    _logger.Verbose("Not routing from '{endPoint}'.");
                    return;
                }

                WaitForInactivityTimeout();
            }
            finally
            {
                if (!queued)
                {
                    ReceiveAsync();
                }
            }
        }

        protected override void OnSent(EndPoint endpoint, long sent)
        {
            ReceiveAsync();
        }

        #endregion

        #region Private Methods

        private void WaitForInactivityTimeout()
        {
            if (_cancellationTokenSource is not null)
            {
                _cancellationTokenSource.CancelAfter(_inactivityTimeout);
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.CancelAfter(_inactivityTimeout);
            _cancellationTokenSource.Token.Register(() =>
            {
                _logger.Debug(
                    "Relay server timed out due to inactivity " +
                    $"(EndPoint='{Endpoint}', " +
                    $"SourceEndPoint='{_sourceEndPoint}', " +
                    $"TargetEndPoint='{_targetEndPoint}')."
                );

                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }

                Stop();
            });
        }

        #endregion
    }
}
