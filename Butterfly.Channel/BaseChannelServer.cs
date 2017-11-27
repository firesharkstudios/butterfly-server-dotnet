﻿/*
 * Copyright 2017 Fireshark Studios, LLC
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using NLog;
using System.Collections.Generic;
using Butterfly.Util;

namespace Butterfly.Channel {
    public class BaseChannelServer {
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected readonly ConcurrentDictionary<string, IChannelTransport> channelTransportById = new ConcurrentDictionary<string, IChannelTransport>();
        protected readonly ConcurrentDictionary<string, Channel> channelById = new ConcurrentDictionary<string, Channel>();

        protected readonly List<Func<Channel, IDisposable>> onNewChannelListeners = new List<Func<Channel, IDisposable>>();
        protected readonly List<Func<Channel, Task<IDisposable>>> onNewChannelAsyncListeners = new List<Func<Channel, Task<IDisposable>>>();

        public IDisposable OnNewChannel(Func<Channel, IDisposable> listener) {
            return new ListItemDisposable<Func<Channel, IDisposable>>(onNewChannelListeners, listener);
        }

        /// <summary>
        /// Calls a user defined function to setup a new channel
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="value"></param>
        /// <param name="path"></param>
        public IDisposable OnNewChannelAsync(Func<Channel, Task<IDisposable>> listener) {
            return new ListItemDisposable<Func<Channel, Task<IDisposable>>>(onNewChannelAsyncListeners, listener);
        }

        /// <summary>
        /// Queues a value to be sent to the path of the specified channel
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="value"></param>
        /// <param name="path"></param>
        public void Queue(string channelId, object value, string path = null) {
            if (!this.channelById.TryGetValue(channelId, out Channel channel)) throw new Exception($"Invalid channel id '{channelId}'");
            channel.Queue(value, path);
        }

        public IChannelTransport GetChannelTransport(string id) {
            if (this.channelTransportById.TryGetValue(id, out IChannelTransport channelTransport)) {
                return channelTransport;
            }
            else {
                return null;
            }
        }

        protected void CreateChannelTransport(string id, Func<IChannelTransport> factory) {
            if (this.channelTransportById.TryGetValue(id, out IChannelTransport existingChannelTransport)) {
                existingChannelTransport.Dispose();
            }
            this.channelTransportById[id] = factory();
        }

        protected void CreateChannel(string id) {
            Channel channel = new Channel(this, id);
            channel.Start(this.onNewChannelListeners, this.onNewChannelAsyncListeners);
            this.channelById[id] = channel;
        }

    }
}
