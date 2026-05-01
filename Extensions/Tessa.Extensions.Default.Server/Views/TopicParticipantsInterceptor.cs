using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tessa.Forums;
using Tessa.Forums.Models;
using Tessa.Platform.Validation;
using Tessa.Scheme;
using Tessa.Views;

namespace Tessa.Extensions.Default.Server.Views
{
    public sealed class TopicParticipantsInterceptor : ViewInterceptorBase
    {
        #region Private Fields

        private readonly IForumPermissionsProvider forumPermissionsProvider;

        #endregion

        #region Constructor

        public TopicParticipantsInterceptor(IForumPermissionsProvider forumPermissionsProvider)
            : base(new[] { ForumHelper.TopicParticipantsView })
            => this.forumPermissionsProvider = forumPermissionsProvider;

        #endregion

        #region Private Methods

        private static TessaViewResult CreateEmptyResult()
        {
            return new TessaViewResult
            {
                Columns = new List<object>(),
                DataTypes = new List<object>(),
                SchemeTypes = new List<SchemeType>(),
                Result = new Dictionary<string, object>(),
                Rows = new List<object>(),
                RowCount = 0,
                HasTimeOut = false,
            };
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc />
        public override async Task<ITessaViewResult> GetDataAsync(ITessaViewRequest request, CancellationToken cancellationToken = default)
        {
            if (!this.InterceptedViews.TryGetValue(
                    request.ViewAlias ?? throw new InvalidOperationException("View alias isn't specified."),
                    out ITessaView view))
            {
                throw new InvalidOperationException($"Can't find view with alias:'{request.ViewAlias}'");
            }

            if (request.TryGetParameter(ForumHelper.TopicIDParam)?.CriteriaValues.FirstOrDefault()?.Values.FirstOrDefault()?.Value is not Guid topicId)
            {
                return CreateEmptyResult();
            }

            (ParticipantModel participant, ValidationResult validationResult) = await this.forumPermissionsProvider.ResolveUserPermissionsAsync(
                topicId,
                checkSuperModeratorMode: true,
                cancellationToken: cancellationToken);

            if (participant is null || !validationResult.IsSuccessful)
            {
                return CreateEmptyResult();
            }

            return await view.GetDataAsync(request, cancellationToken);
        }

        #endregion
    }
}
