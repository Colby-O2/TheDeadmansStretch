using DialogueGraph.Data;
using InteractionSystem.UI;
using PlazmaGames.Attribute;
using PlazmaGames.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ColbyO.Untitled.UI
{
    public class DialogueView : View
    {
        [Header("Dialogue UI")]
        [SerializeField] private GameObject _dialogueHolder;
        [SerializeField] private TMP_Text _dialogueAvatarName;
        [SerializeField] private TMP_Text _dialogueText;
        [SerializeField] private GameObject _dialogueHint;

        [Header("Settings")]
        [SerializeField] private float _typeSpeed = 0.05f;
        [SerializeField] private float _delayBetweenDialogues = 3f;

        [Header("Choice UI")]
        [SerializeField] private GameObject _choiceHolder;
        [SerializeField] private List<UIIcon> _choiceIcons;
        [SerializeField] private List<TMP_Text> _choiceTexts;
        [SerializeField] private List<Image> _choiceBackgrounds;

        [Header("Input")]
        [SerializeField] private InputAction _nextAction;
        [SerializeField] private List<InputAction> _choiceActions;

        private Coroutine _typeRoutine;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool _isTyping;
        [SerializeField, ReadOnly] private bool _isWaitingForInput;
        [SerializeField, ReadOnly] private bool _isPassive;
        [SerializeField, ReadOnly] string _fullCurrentMessage;
        [SerializeField, ReadOnly] private float _timeWaitingForInput;

        public UnityEvent<int> OnChoiceSelected { get; private set; } 
        public UnityEvent OnRequestNext { get; private set; }

        private void Awake()
        {
            OnChoiceSelected = new UnityEvent<int>();
            OnRequestNext = new UnityEvent();

            OnRequestNext.AddListener(OnDiagoueEnd);
        }

        private void Update()
        {
            UpdateIcons();

            if (UTGameManager.IsPaused) return;

            if (_isWaitingForInput)
            {
                _timeWaitingForInput += Time.deltaTime;
                if (_timeWaitingForInput > _delayBetweenDialogues) 
                {
                    _timeWaitingForInput = 0.0f;
                    OnRequestNext?.Invoke();
                }
            }
        }

        public override void Init()
        {
            HideDialogue();
            HideChoice();
            RegisterInputs();
        }

        public override void Show()
        {
            base.Show();
            _nextAction.Enable();
        }
        public override void Hide()
        {
            _nextAction.Disable();
            DisableChoiceInput();
            HideChoice();
            HideDialogue();
        }

        private void RegisterInputs()
        {
            _nextAction.performed += _ => HandleNextPressed();

            for (int i = 0; i < _choiceActions.Count; i++)
            {
                int index = i;
                _choiceActions[i].performed += _ => OnChooseChoice(index);
            }
        }

        private void UpdateIcons()
        {
            foreach (UIIcon icon in _choiceIcons)
            {
                if (icon.IsActive()) icon.UpdateIconMaterial();
            }
        }

        public void DisplayMessage(string actor, string message, bool passive = false)
        {
            HideChoice();

            _isPassive = passive;
            _fullCurrentMessage = message;
            _dialogueAvatarName.text = actor;

            _dialogueHolder.SetActive(true);
            _dialogueHint.SetActive(false);

            if (_typeRoutine != null) StopCoroutine(_typeRoutine);
            _typeRoutine = StartCoroutine(TypewriterRoutine(message));
        }

        public void DisplayChoices(List<DialogueChoiceData> choices)
        {
            HideDialogue();

            _isWaitingForInput = false;
            _dialogueHint.SetActive(false);
            _choiceHolder.SetActive(true);

            for (int i = 0; i < _choiceTexts.Count; i++)
            {
                bool isActive = i < choices.Count;
                _choiceTexts[i].gameObject.SetActive(isActive);
                if (isActive)
                {
                    _choiceTexts[i].text = choices[i].Text;
                    _choiceIcons[i].SetActive(true);
                    _choiceActions[i].Enable();
                }
                else
                {
                    _choiceTexts[i].text = string.Empty;
                    _choiceIcons[i].SetActive(false);
                    if (_choiceBackgrounds[i].gameObject.activeSelf) _choiceBackgrounds[i].gameObject.SetActive(false);
                    _choiceActions[i].Disable();
                }
            }
        }

        public void HideDialogue()
        {
            if (_typeRoutine != null || _isTyping) CompleteTyping();
            _dialogueHolder.SetActive(false);
            _isTyping = false;
            _isWaitingForInput = false;
        }

        public void HideChoice()
        {
            _choiceHolder.SetActive(false);
            DisableChoiceInput();
        }

        private IEnumerator TypewriterRoutine(string text)
        {
            if (_isPassive) 
            {
                _dialogueText.text = text;
            }
            else
            {
                _isTyping = true;
                _dialogueText.text = "";
                int visibleCharacters = 0;

                while (visibleCharacters < text.Length)
                {
                    while (UTGameManager.IsPaused)
                    {
                        yield return null;
                    }

                    if (text[visibleCharacters] == '<')
                    {
                        int endTag = text.IndexOf('>', visibleCharacters);
                        if (endTag != -1) visibleCharacters = endTag + 1;
                    }
                    else
                    {
                        visibleCharacters++;
                    }

                    _dialogueText.text = text.Substring(0, visibleCharacters);
                    yield return new WaitForSeconds(_typeSpeed);
                }
            }
            
            CompleteTyping();
        }

        private void OnDiagoueEnd()
        {
            HideDialogue();
            _isWaitingForInput = false;
            _timeWaitingForInput = 0.0f;
            OnChoiceSelected?.Invoke(0);
        }

        private void CompleteTyping()
        {
            if (_typeRoutine != null) StopCoroutine(_typeRoutine);

            _dialogueText.text = _fullCurrentMessage;
            _isTyping = false;
            _isWaitingForInput = true;
            _timeWaitingForInput = 0.0f;
        }

        private void HandleNextPressed()
        {
            if (UTGameManager.IsPaused) return;

            if (_isTyping)
            {
                CompleteTyping();
            }
            else if (_isWaitingForInput)
            {
                _isWaitingForInput = false;
                OnRequestNext?.Invoke();
            }
        }

        private void OnChooseChoice(int index)
        {
            if (!_choiceHolder.activeSelf || index >= _choiceTexts.Count) return;

            HideChoice();
            OnChoiceSelected?.Invoke(index);
        }

        private void EnableChoiceInput()
        {
            foreach (InputAction action in _choiceActions) action.Enable();
        }

        private void DisableChoiceInput()
        {
            foreach (InputAction action in _choiceActions) action.Disable();
        }
    }
}