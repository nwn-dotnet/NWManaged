using NWN.Core;

namespace Anvil.API
{
  public enum EffectType
  {
    InvalidEffect = NWScript.EFFECT_TYPE_INVALIDEFFECT,
    DamageResistance = NWScript.EFFECT_TYPE_DAMAGE_RESISTANCE,
    Regenerate = NWScript.EFFECT_TYPE_REGENERATE,
    DamageReduction = NWScript.EFFECT_TYPE_DAMAGE_REDUCTION,
    TemporaryHitpoints = NWScript.EFFECT_TYPE_TEMPORARY_HITPOINTS,
    Entangle = NWScript.EFFECT_TYPE_ENTANGLE,
    Invulnerable = NWScript.EFFECT_TYPE_INVULNERABLE,
    Deaf = NWScript.EFFECT_TYPE_DEAF,
    Resurrection = NWScript.EFFECT_TYPE_RESURRECTION,
    Immunity = NWScript.EFFECT_TYPE_IMMUNITY,
    EnemyAttackBonus = NWScript.EFFECT_TYPE_ENEMY_ATTACK_BONUS,
    ArcaneSpellFailure = NWScript.EFFECT_TYPE_ARCANE_SPELL_FAILURE,
    AreaOfEffect = NWScript.EFFECT_TYPE_AREA_OF_EFFECT,
    Beam = NWScript.EFFECT_TYPE_BEAM,
    Charmed = NWScript.EFFECT_TYPE_CHARMED,
    Confused = NWScript.EFFECT_TYPE_CONFUSED,
    Frightened = NWScript.EFFECT_TYPE_FRIGHTENED,
    Dominated = NWScript.EFFECT_TYPE_DOMINATED,
    Paralyze = NWScript.EFFECT_TYPE_PARALYZE,
    Dazed = NWScript.EFFECT_TYPE_DAZED,
    Stunned = NWScript.EFFECT_TYPE_STUNNED,
    Sleep = NWScript.EFFECT_TYPE_SLEEP,
    Poison = NWScript.EFFECT_TYPE_POISON,
    Disease = NWScript.EFFECT_TYPE_DISEASE,
    Curse = NWScript.EFFECT_TYPE_CURSE,
    Silence = NWScript.EFFECT_TYPE_SILENCE,
    Turned = NWScript.EFFECT_TYPE_TURNED,
    Haste = NWScript.EFFECT_TYPE_HASTE,
    Slow = NWScript.EFFECT_TYPE_SLOW,
    AbilityIncrease = NWScript.EFFECT_TYPE_ABILITY_INCREASE,
    AbilityDecrease = NWScript.EFFECT_TYPE_ABILITY_DECREASE,
    AttackIncrease = NWScript.EFFECT_TYPE_ATTACK_INCREASE,
    AttackDecrease = NWScript.EFFECT_TYPE_ATTACK_DECREASE,
    DamageIncrease = NWScript.EFFECT_TYPE_DAMAGE_INCREASE,
    DamageDecrease = NWScript.EFFECT_TYPE_DAMAGE_DECREASE,
    DamageImmunityIncrease = NWScript.EFFECT_TYPE_DAMAGE_IMMUNITY_INCREASE,
    DamageImmunityDecrease = NWScript.EFFECT_TYPE_DAMAGE_IMMUNITY_DECREASE,
    AcIncrease = NWScript.EFFECT_TYPE_AC_INCREASE,
    AcDecrease = NWScript.EFFECT_TYPE_AC_DECREASE,
    MovementSpeedIncrease = NWScript.EFFECT_TYPE_MOVEMENT_SPEED_INCREASE,
    MovementSpeedDecrease = NWScript.EFFECT_TYPE_MOVEMENT_SPEED_DECREASE,
    SavingThrowIncrease = NWScript.EFFECT_TYPE_SAVING_THROW_INCREASE,
    SavingThrowDecrease = NWScript.EFFECT_TYPE_SAVING_THROW_DECREASE,
    SpellResistanceIncrease = NWScript.EFFECT_TYPE_SPELL_RESISTANCE_INCREASE,
    SpellResistanceDecrease = NWScript.EFFECT_TYPE_SPELL_RESISTANCE_DECREASE,
    SkillIncrease = NWScript.EFFECT_TYPE_SKILL_INCREASE,
    SkillDecrease = NWScript.EFFECT_TYPE_SKILL_DECREASE,
    Invisibility = NWScript.EFFECT_TYPE_INVISIBILITY,
    ImprovedInvisibility = NWScript.EFFECT_TYPE_IMPROVEDINVISIBILITY,
    Darkness = NWScript.EFFECT_TYPE_DARKNESS,
    DispelMagical = NWScript.EFFECT_TYPE_DISPELMAGICALL,
    ElementalShield = NWScript.EFFECT_TYPE_ELEMENTALSHIELD,
    NegativeLevel = NWScript.EFFECT_TYPE_NEGATIVELEVEL,
    Polymorph = NWScript.EFFECT_TYPE_POLYMORPH,
    Sanctuary = NWScript.EFFECT_TYPE_SANCTUARY,
    TrueSeeing = NWScript.EFFECT_TYPE_TRUESEEING,
    SeeInvisible = NWScript.EFFECT_TYPE_SEEINVISIBLE,
    TimeStop = NWScript.EFFECT_TYPE_TIMESTOP,
    Blindness = NWScript.EFFECT_TYPE_BLINDNESS,
    SpellLevelAbsorption = NWScript.EFFECT_TYPE_SPELLLEVELABSORPTION,
    DispelMagicBest = NWScript.EFFECT_TYPE_DISPELMAGICBEST,
    Ultravision = NWScript.EFFECT_TYPE_ULTRAVISION,
    MissChance = NWScript.EFFECT_TYPE_MISS_CHANCE,
    Concealment = NWScript.EFFECT_TYPE_CONCEALMENT,
    SpellImmunity = NWScript.EFFECT_TYPE_SPELL_IMMUNITY,
    VisualEffect = NWScript.EFFECT_TYPE_VISUALEFFECT,
    DisappearAppear = NWScript.EFFECT_TYPE_DISAPPEARAPPEAR,
    Swarm = NWScript.EFFECT_TYPE_SWARM,
    TurnResistanceDecrease = NWScript.EFFECT_TYPE_TURN_RESISTANCE_DECREASE,
    TurnResistanceIncrease = NWScript.EFFECT_TYPE_TURN_RESISTANCE_INCREASE,
    Petrify = NWScript.EFFECT_TYPE_PETRIFY,
    CutsceneParalyze = NWScript.EFFECT_TYPE_CUTSCENE_PARALYZE,
    Ethereal = NWScript.EFFECT_TYPE_ETHEREAL,
    SpellFailure = NWScript.EFFECT_TYPE_SPELL_FAILURE,
    CutsceneGhost = NWScript.EFFECT_TYPE_CUTSCENEGHOST,
    CutsceneImmobilize = NWScript.EFFECT_TYPE_CUTSCENEIMMOBILIZE,
    RunScript = NWScript.EFFECT_TYPE_RUNSCRIPT,
    Icon = NWScript.EFFECT_TYPE_ICON,
    Pacify = NWScript.EFFECT_TYPE_PACIFY,
  }
}
