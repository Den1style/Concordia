using Concordia.Entities;

namespace Concordia.Managers.Interfaces
{
    interface IManager
    {
        void AddMessageToManager(BotCommand command);
        void Init();
    }
}
