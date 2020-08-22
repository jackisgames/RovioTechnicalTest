using System.Collections.Generic;
using Assets.Scripts.Presentation.Entities;
using Assets.Scripts.Presentation.Levels;
using Assets.Scripts.UI;
using Logic.Gameplay.Entity;
using Logic.Gameplay.Level;
using Logic.Gameplay.Skill;
using Logic.Gameplay.State;
using Logic.Navigation;
using UnityEngine;

namespace Logic.Gameplay
{
    class GameManager:MonoBehaviour
    {
        public readonly List<GameplayEntity> GameplayEntities = new List<GameplayEntity>();
        public readonly List<GameplayEntity> PlayerEntities = new List<GameplayEntity>();
        public readonly List<GameplayEntity> EnemiesEntities = new List<GameplayEntity>();

        public readonly Pathfinder Pathfinder = new Pathfinder();
        public readonly StateManager StateManager = new StateManager();

        private readonly LevelManager m_levelManager = new LevelManager();
        private readonly SkillManager m_skillManager = new SkillManager();

        public readonly RoundInfo RoundInfo = new RoundInfo();

        public LevelService Service;
        public UiComponent Ui;

        [System.NonSerialized]
        private int m_currentLevel = 1;

        public void Initialize(LevelService levelService,UiComponent ui)
        {
            this.Service = levelService;

            this.Ui = ui;

            m_skillManager.Initialize(this);

            StateManager.Initialize(this);

            Ui.RetryButton.onClick.AddListener(Retry);
            Ui.NextLevelButton.onClick.AddListener(NextLevel);

            InitializeMatch();
        }

        
        private void Retry()
        {
            Service.LoadLastLevel();
            InitializeMatch();
        }

        private void NextLevel()
        {
            m_currentLevel++;
            Service.LoadLevelData(m_levelManager.GetLevelData(m_currentLevel));
            InitializeMatch();
        }

        public GameplayEntity RegisterEntity(EntityComponent visualEntity)
        {
            GameplayEntity entity = new GameplayEntity(visualEntity);

            entity.Status.MaxTraverseCount = 7;//6 grids + current grid

            entity.Skills.Add(m_skillManager.GetSkill<BasicAttack>(entity));

            //assign special skills
            if (Random.value < .5f)
            {
                entity.Skills.Add(m_skillManager.GetSkill<PushAttack>(entity));
            }

            if (Random.value < .5f)
            {
                entity.Skills.Add(m_skillManager.GetSkill<Reinforcement>(entity));
            }

            if (Random.value < .5f)
            {
                entity.Skills.Add(m_skillManager.GetSkill<GroundSlam>(entity));
            }

            if (Random.value < .5f)
            {
                entity.Skills.Add(m_skillManager.GetSkill<Convert>(entity));
            }

            if (Random.value < .5f)
            {
                entity.Skills.Add(m_skillManager.GetSkill<Heal>(entity));
            }

            GameplayEntities.Add(entity);

            if (entity.Visual.Type == EntityType.Player)
            {
                PlayerEntities.Add(entity);
            }
            else if (entity.Visual.Type == EntityType.Enemy)
            {
                EnemiesEntities.Add(entity);
            }

            return entity;
        }

        public void InitializeMatch()
        {
            Ui.SkipTurnButton.onClick.RemoveAllListeners();
            Ui.SkillsDropdown.onValueChanged.RemoveAllListeners();

            Pathfinder.Load(new NavigationGraph(Service.LevelData));

            StateManager.Reset();

            GameplayEntities.Clear();
            EnemiesEntities.Clear();
            PlayerEntities.Clear();

            for (int i = 0; i < Service.LevelData.Entities.Count; i++)
            {
                RegisterEntity(Service.LevelData.Entities[i]);
            }

            Ui.GameoverScreen.SetActive(false);
            StateManager.RegisterState<StartRoundState>();
        }

        public void Update()
        {
            StateManager.Tick();
        }

        public GameplayEntity GetEntityAtPosition(Vector2Int position,List<GameplayEntity> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                GameplayEntity entity = list[i];
                if (entity.Visual.GridPosition == position)
                {
                    return entity;
                }
            }

            return null;
        }
    }

    class RoundInfo
    {
        public int CurrentRound;
        public int PlayerRound;
        public int EnemyRound;
    }
}
