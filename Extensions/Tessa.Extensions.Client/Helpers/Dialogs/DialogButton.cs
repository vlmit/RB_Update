using System;
using System.Windows;
using Tessa.UI;
using Tessa.UI.Cards;

namespace Tessa.Extensions.Client.Helpers.Dialogs
{
    public class DialogButton
    {
        public string Caption { get; }
        public Action<ICardModel, UIButton> ButtonFunc { get; }
        public Func<bool> IsEnabled { get; }

        public Visibility Visibility { get; set; }

        public DialogButton(string caption, Action<ICardModel, UIButton> buttonFunc = null, Func<bool> isEnabled = null, Visibility visibility = Visibility.Visible)
        {
            this.Caption = caption;

            buttonFunc ??= async (x, b) => await b.CloseAsync();
            this.ButtonFunc = buttonFunc;

            this.IsEnabled = isEnabled;
            this.Visibility = visibility;
        }

        public static readonly DialogButton CancelButton = new("Отмена");
    }
}