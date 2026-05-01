using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Tessa.Cards;
using Tessa.Extensions.Shared.Info;
using Tessa.UI.Cards;

namespace Tessa.Extensions.Client.Helpers.Dialogs
{
    public interface IDialogHelper
    {
        Task ShowDialogAsync(
            string cardTypeName,
            string name,
            string caption = null,
            Func<IFormViewModel, CancellationToken, ValueTask> initFormAction = null,
            Action<Card> initCardAction = null,
            Func<Window, CancellationToken, ValueTask> initWindowAction = null,
            params DialogButton[] dialogButtons);

      //  Task<string> GetCommentaryAsync(string acceptButtonCaption);

       /* Task<ResultWithCommentary<List<User>>> GetUserAsync(
            string acceptButtonCaption,
            bool hasCommentary,
            bool commentaryRequired = false,
            string forwardComment = null,
            string userFieldCaption = null,
            string formCaption = null,
            bool isMultiPerformer = false);

        Task<ResultsWithCommentary<User>> GetDistributionUsersAsync(
            string acceptButtonCaption,
            bool hasCommentary,
            bool commentaryRequired = false,
            string forwardComment = null,
            string userFieldCaption = null,
            string formCaption = null);*/
    }
}