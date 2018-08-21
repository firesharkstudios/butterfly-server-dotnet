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
using System.Threading.Tasks;

using Dict = System.Collections.Generic.Dictionary<string, object>;

namespace Butterfly.Core.Channel {
    /// <summary>
    /// Represents a subscription to a specific channel
    /// </summary>
    public class ChannelSubscription {
        public readonly string channelKey;
        public readonly Func<Dict, Channel, IDisposable> handle;
        public readonly Func<Dict, Channel, Task<IDisposable>> handleAsync;

        public ChannelSubscription(string channelKey, Func<Dict, Channel, IDisposable> handle) {
            this.channelKey = channelKey;
            this.handle = handle;
            this.handleAsync = null;
        }

        public ChannelSubscription(string key, Func<Dict, Channel, Task<IDisposable>> handleAsync) {
            this.channelKey = key;
            this.handle = null;
            this.handleAsync = handleAsync;
        }
    }
}
