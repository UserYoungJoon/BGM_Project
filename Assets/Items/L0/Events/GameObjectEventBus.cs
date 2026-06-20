using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace YoungJoon.L0.Events
{
    public class GameObjectEventBus : MonoBehaviour
    {
        private Dictionary<Type, List<Delegate>> _subscriptions = new Dictionary<Type, List<Delegate>>();
        // DisconnectEvent 발생 횟수. 중첩 SendEvent에서도 외부가 inner의 disconnect를 감지하기 위해 monotonic 증가.
        private uint _disconnectVersion = 0;

        public Action<T> ConnectEvent<T>(Action<T> handler) where T : struct, IGameEvent
        {
            if (!_subscriptions.ContainsKey(typeof(T)))
                _subscriptions[typeof(T)] = new List<Delegate>();

            // 중복 방지: 이미 등록된 핸들러는 다시 등록하지 않음
            if (!_subscriptions[typeof(T)].Contains(handler))
                _subscriptions[typeof(T)].Add(handler);

            return handler;
        }

        public void DisconnectEvent<T>(Action<T> handler) where T : struct, IGameEvent
        {
            if (_subscriptions.TryGetValue(typeof(T), out var subs))
            {
                if (subs.Remove(handler))
                    _disconnectVersion++;
            }
        }

        public void ConnectEventOnce<T>(Action<T> handler) where T : struct, IGameEvent
        {
            Action<T> wrapper = null;
            wrapper = (evt) =>
            {
                handler(evt);
                DisconnectEvent(wrapper);
            };

            ConnectEvent(wrapper);
        }

        public void SendEvent<T>(T eventData) where T : struct, IGameEvent
        {
            if (!_subscriptions.TryGetValue(typeof(T), out var handlers))
                return;

            int count = handlers.Count;
            if (count == 0) return;

            // 시작 시점의 version을 local에 캡처. 중첩 SendEvent가 reset해도 outer의 versionAtStart는 변하지 않음.
            uint versionAtStart = _disconnectVersion;

            var pool = ArrayPool<Delegate>.Shared; // 이벤트 내에서 이벤트 해제가 일어날 수 있으므로 위험
            var snapshot = pool.Rent(count);
            bool hasDirty = false;
            try
            {
                handlers.CopyTo(snapshot, 0);
                for (int i = 0; i < count; i++)
                {
                    if (hasDirty)
                    {
                        if (IsInCurrentHandler(snapshot[i], handlers))
                        {
                            ((Action<T>)snapshot[i]).Invoke(eventData);
                        }
                    }
                    else
                    {
                        ((Action<T>)snapshot[i]).Invoke(eventData);
                    }

                    if (_disconnectVersion != versionAtStart)
                    {
                        hasDirty = true;
                    }
                }
            }
            finally
            {
                // Delegate 참조가 풀에 남아 GC 막는 것 방지
                Array.Clear(snapshot, 0, count);
                pool.Return(snapshot);
            }
        }

        // 현재 시점의 handler 리스트에 procedure가 살아있는지 O(n) 선형 탐색.
        // dirty가 감지된 후에만 호출되므로 일반 케이스 비용 0.
        private bool IsInCurrentHandler(Delegate procedure, List<Delegate> currentHandlers)
        {
            for (int i = 0; i < currentHandlers.Count; i++)
            {
                if (ReferenceEquals(currentHandlers[i], procedure))
                    return true;
            }
            return false;
        }

        private void OnDestroy()
        {
            _subscriptions.Clear();
        }
    }
}
