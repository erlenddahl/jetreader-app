using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using JetReader.Model.Messages;
using JetReader.Service;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Home {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddBookCard : StackLayout {
        public AddBookCard() {

            BindingContext = new {
                Width = Card.CardWidth,
            };

            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e) {
            var messageBus = IocManager.Container.Resolve<IMessageBus>();
            messageBus.Send(new AddBookClickedMessage());
        }
    }
}