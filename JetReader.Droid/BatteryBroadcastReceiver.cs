using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Autofac;
using JetReader.Model.Messages;
using JetReader.Service;

namespace JetReader.Droid {
    public class BatteryBroadcastReceiver : BroadcastReceiver {
        public override void OnReceive(Context context, Intent intent) {
            var messageBus = IocManager.Container.Resolve<IMessageBus>();
            messageBus.Send(new BatteryChangeMessage { });
        }
    }
}