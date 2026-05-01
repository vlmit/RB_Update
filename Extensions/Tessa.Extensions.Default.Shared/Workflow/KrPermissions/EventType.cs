
namespace Tessa.Extensions.Default.Shared.Workflow.KrPermissions
{
    public enum EventType
    {
        /// <summary>
        /// При загрузке карточки (карточка уже загружена)
        /// </summary>
        Loading = 0,

        /// <summary>
        /// При загрузке контента файла или списка его версий (карточка не загружена).
        /// </summary>
        LoadingFile = 1,

        /// <summary>
        /// При сохранении карточки (карточка не загружена).
        /// </summary>
        Store = 2,
    }
}
