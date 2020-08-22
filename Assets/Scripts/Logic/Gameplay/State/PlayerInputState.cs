using System.Collections.Generic;
using Assets.Scripts.Presentation.Levels;
using Logic.Gameplay.Entity;
using Logic.Gameplay.Skill;
using UnityEngine;
using UnityEngine.UI;

namespace Logic.Gameplay.State
{
    class PlayerInputState:IGameState
    {

        public bool ShouldEnd { get; private set; }

        private readonly List<Vector2Int> m_pathBuffer = new List<Vector2Int>();

        private readonly List<GameplayEntity> m_selectablePlayer = new List<GameplayEntity>();

        private readonly List<GameplayEntity> m_targetableEntities = new List<GameplayEntity>();

        private readonly List<Dropdown.OptionData> m_DropdownOptions = new List<Dropdown.OptionData>();

        private GameManager m_manager;

        private GameplayEntity m_activeEntity;

        private Vector2Int m_previousMouse;

        private bool m_forceRepath;

        private int m_skillSelected;

        public void Initialize(GameManager manager)
        {
            m_manager = manager;            
        }

        public void Begin()
        {
            //get list of selectable player
            for (int i = 0; i < m_manager.PlayerEntities.Count; i++)
            {
                GameplayEntity entity = m_manager.PlayerEntities[i];
                if (entity.Status.Health > 0)
                {
                    if (entity.HaveAction)
                    {
                        m_selectablePlayer.Add(entity);
                    }
                }
            }

            if (m_selectablePlayer.Count == 0)
            {
                //end this state
                ShouldEnd = true;
            }
            else
            {
                ShouldEnd = false;
            }

            m_manager.Ui.SkipTurnButton.onClick.AddListener(OnSkipTurnClicked);
            m_manager.Ui.SkillsDropdown.onValueChanged.AddListener(OnDropDownValueChanged);
            
            m_manager.Ui.SkipTurnButton.gameObject.SetActive(true);
            m_manager.Ui.SkillsDropdownCanvasGroup.alpha = 0;
        }
        
        public void Tick()
        {
            Vector2Int currentMousePosition = LevelGrid.MouseToGridCoordinates();

            GameplayEntity selectedPlayerEntity = m_manager.GetEntityAtPosition(currentMousePosition, m_selectablePlayer);

            if (Input.GetMouseButtonDown(0)&&
                selectedPlayerEntity != null && 
                selectedPlayerEntity != m_activeEntity&&
                !selectedPlayerEntity.Visual.AttackTargetSelection.activeSelf)
            {
                m_activeEntity?.Visual.ShowSelection(false);

                m_activeEntity = selectedPlayerEntity;

                m_activeEntity.Visual.ShowSelection(true);

                if (m_activeEntity.TurnState.CanAttack)
                {
                    m_DropdownOptions.Clear();
                    for (int i = 0; i < m_activeEntity.Skills.Count; i++)
                    {
                        m_DropdownOptions.Add(new Dropdown.OptionData(m_activeEntity.Skills[i].Name));
                    }

                    m_skillSelected = 0;
                    Dropdown dropdown = m_manager.Ui.SkillsDropdown;
                    dropdown.ClearOptions();
                    dropdown.AddOptions(m_DropdownOptions);
                    dropdown.value = m_skillSelected;
                    
                    dropdown.RefreshShownValue();

                    CanvasGroup canvasGroup = dropdown.GetComponentInChildren<CanvasGroup>();//<-fix for unity bug
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = 1;
                    }

                    m_manager.Ui.SkillsDropdownCanvasGroup.alpha = 1;
                    UpdateSkillTarget();
                }
                else
                {
                    m_manager.Ui.SkillsDropdownCanvasGroup.alpha = 0;
                    ToggleTarget(false);
                    m_targetableEntities.Clear();
                }

                m_forceRepath = true;
                return;
            }

            if (m_activeEntity != null)
            {
                if (m_activeEntity.TurnState.CanAttack)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        GameplayEntity entity = m_manager.GetEntityAtPosition(currentMousePosition,m_targetableEntities);
                        if (entity != null)
                        {
                            //reset
                            m_manager.Service.HideAllBreadCrumbs();
                            ToggleTarget(false);
                            //attack state
                            m_activeEntity.Visual.ShowSelection(false);
                            m_activeEntity.Skills[m_skillSelected].Execute(entity);
                            m_activeEntity.TurnState.CanAttack = false;
                            ShouldEnd = true;
                            return;
                        }
                    }
                }

                if (m_activeEntity.TurnState.CanMove)
                {
                    if (m_forceRepath || currentMousePosition != m_previousMouse)
                    {
                        m_manager.Pathfinder.Navigate(m_activeEntity.Visual.GridPosition, currentMousePosition, m_pathBuffer);

                        m_activeEntity.ValidateMovement(m_pathBuffer);

                        m_manager.Service.HideAllBreadCrumbs();

                        for (int i = 1; i < m_pathBuffer.Count; i++)
                        {
                            Vector2Int coord = m_pathBuffer[i];
                            m_manager.Service.ShowBreadCrumb(coord.x, coord.y, true, i*.05f);
                        }

                        m_previousMouse = currentMousePosition;
                        m_forceRepath = false;
                    }

                    if (Input.GetMouseButtonDown(0) && m_pathBuffer.Count >= 2)
                    {
                        //move
                        ToggleTarget(false);
                        m_activeEntity.Visual.ShowSelection(false);
                        m_manager.StateManager.RegisterState<EntityMoveState>().Move(m_activeEntity, m_pathBuffer);
                        m_activeEntity.TurnState.CanMove = false;
                        ShouldEnd = true;
                        return;
                    }
                }
                
            }
        }

        private void OnSkipTurnClicked()
        {
            for (int i = 0; i < m_selectablePlayer.Count; i++)
            {
                GameplayEntity entity = m_selectablePlayer[i];
                entity.Visual.ShowSelection(false);
                entity.TurnState.CanAttack = entity.TurnState.CanMove = false;
            }

            ShouldEnd = true;
            m_manager.StateManager.RegisterState<StartTurnState>();
        }

        private void OnDropDownValueChanged(int selected)
        {
            m_skillSelected = selected;
            UpdateSkillTarget();
        }

        private void UpdateSkillTarget()
        {
            ToggleTarget(false);

            ISkill skill = m_activeEntity.Skills[m_skillSelected];
            m_manager.Ui.SkilDescriptionText.text = skill.Description;        
            skill.GetValidTargets(m_targetableEntities);

            ToggleTarget(true);
        }

        private void ToggleTarget(bool show)
        {
            for (int i = 0; i < m_targetableEntities.Count; i++)
            {
                m_targetableEntities[i].Visual.ShowSelectionAttackTarget(show);
            }
        }

        public void End()
        {
            ToggleTarget(false);

            m_manager.Ui.SkillsDropdown.onValueChanged.RemoveListener(OnDropDownValueChanged);
            m_manager.Ui.SkipTurnButton.onClick.RemoveListener(OnSkipTurnClicked);
            m_manager.Ui.SkillsDropdownCanvasGroup.alpha = 0;
            m_manager.Ui.SkipTurnButton.gameObject.SetActive(false);
        }

    }
}
