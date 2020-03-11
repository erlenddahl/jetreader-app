using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

namespace EbookReader.Extensions
{
    public static class PopupPageExtensions
    {
        public static async Task Hide(this PopupPage page, bool animate = false)
        {
            if (PopupNavigation.Instance.PopupStack.Contains(page))
                await PopupNavigation.Instance.RemovePageAsync(page, animate);
        }

        public static async Task Show(this PopupPage page, bool animate = false)
        {
            if (!PopupNavigation.Instance.PopupStack.Contains(page))
                await PopupNavigation.Instance.PushAsync(page, animate);
        }
    }
}
