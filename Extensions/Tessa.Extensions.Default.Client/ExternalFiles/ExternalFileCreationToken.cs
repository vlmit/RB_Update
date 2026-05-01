using Tessa.Files;

namespace Tessa.Extensions.Default.Client.ExternalFiles
{
    public class ExternalFileCreationToken : FileCreationToken
    {
        #region Properties

        public string Description { get; set; }

        #endregion

        #region Base Overrides

        protected override void SetCore(IFileCreationToken token)
        {
            base.SetCore(token);

            var typedToken = token as ExternalFileCreationToken;
            this.Description = typedToken != null ? typedToken.Description : null;
        }


        protected override void SetCore(IFile file)
        {
            base.SetCore(file);

            var typedFile = file as ExternalFile;
            this.Description = typedFile != null ? typedFile.Description : null;
        }

        #endregion
    }
}
