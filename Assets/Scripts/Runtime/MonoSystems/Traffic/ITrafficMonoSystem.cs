using PlazmaGames.Core.MonoSystem;

namespace ColbyO.Untitled
{
    public interface ITrafficMonoSystem : IMonoSystem
    {
        public void DisableLeftLane(bool state);
        public bool Enabled { get; set; }
    }
}