using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetReader.Service {
    public interface IMessageBus {
        void Send<T>(T message);
        void Subscribe<T>(Action<T> action, params string[] tags);
        void UnSubscribe(string tag);
    }
}
