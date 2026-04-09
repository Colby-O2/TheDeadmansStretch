using ColbyO.Untitled.UI;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ColbyO.Untitled.MonoSystems
{
    public class TaskMonoSystem : MonoBehaviour, ITaskMonoSystem
    {
        [SerializeField, ReadOnly] private bool _hasTask;
        [SerializeField, ReadOnly] private string _taskMsg;
        [SerializeField, ReadOnly] private int _maxCount;
        [SerializeField, ReadOnly] private int _count;

        private Promise _promise;
        private GameView _gameView;

        private void Start()
        {
            _gameView = GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>();
        }

        private string GetTaskString()
        {
            return (_maxCount > 0) ? $"{_taskMsg} {_count}/{_maxCount}" : _taskMsg;
        }

        public Promise StartTask(string msg, int maxCount = -1)
        {
            if (_promise != null)
            {
                Debug.LogWarning("TaskMonoSystem already has a task in progress. Task is ignoring.");
                return null;
            }

            _hasTask = true;
            _maxCount = maxCount;
            _taskMsg = msg;
            _count = 0;

            //_gameView.ShowTask(GetTaskString());

            return Promise.CreateExisting(ref _promise);
        }

        public void UpdateTask(bool preventAutoEnding = false)
        {
            _count++;
            //_gameView.UpdateTask(GetTaskString());
            if (!preventAutoEnding && _maxCount <= _count) EndTask();
        }

        public void EndTask()
        {
            _hasTask = false;
            _maxCount = -1;
            _count = 0;
            _taskMsg = string.Empty;
            Promise.ResolveExisting(ref _promise);
        }
    }
}