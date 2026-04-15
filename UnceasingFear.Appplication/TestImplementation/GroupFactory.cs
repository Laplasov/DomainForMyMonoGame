using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnceasingFear.Domain.Combat.Enums;
using UnceasingFear.Domain.Shared.ValueObjects;
using UnceasingFear.Domain.Shared.ValueObjects.Abilities;
using UnceasingFear.Domain.Shared.ValueObjects.Stats;
using UnceasingFear.Domain.World.Aggregates;
using UnceasingFear.Domain.World.Entities;
using UnceasingFear.Domain.World.Enums;
using UnceasingFear.Domain.World.ValueObjects;

namespace UnceasingFear.TestImplementation
{
    public class GroupFactory
    {
        public static Group CreateGroup1Goblin()
        {

            var ability = Ability.Create(
                        id: "base_attack",
                        name: "BaseAttack",
                        description: "Simple attack",
                        range: TargetRange.Melee,
                        target: Target.Enemy,
                        scales: new List<Scale>(),
                        costs: new List<Cost>(),
                        statusEffects: new List<Status>()
                        );

            var newAbilitis = new List<Ability>();
            newAbilitis.Add(ability);

            var newProfile = UnitProfile.Create(
                    name: "Goblin",
                    slot: 1,
                    stats: UnitStats.Create(
                        maxHealth: 20,
                        maxSP: 10,
                        physic: 5,
                        defense: 3,
                        magic: 1,
                        speed: 15),
                    abilities: newAbilitis
                    );
            var groupTemplate = new List<UnitProfile>();
            groupTemplate.Add(newProfile);

            return new Group(
                id: new GroupId("Goblin"),
                template: new Template("Goblin", groupTemplate),
                movementPattern: MovementPattern.Stationary,
                aggroRange: new AggroRange(250f),
                speed: new MovementSpeed(6f),
                startPosition: new WorldPosition(400, 300)
            );
        }

        public static Group CreateGroup2Slime()
        {

            var ability = Ability.Create(
                        id: "base_attack",
                        name: "BaseAttack",
                        description: "Simple attack",
                        range: TargetRange.Melee,
                        target: Target.Enemy,
                        scales: new List<Scale>(),
                        costs: new List<Cost>(),
                        statusEffects: new List<Status>()
                        );

            var newAbilitis = new List<Ability>();
            newAbilitis.Add(ability);

            var newProfile = UnitProfile.Create(
                    name: "Slime",
                    slot: 1,
                    stats: UnitStats.Create(
                        maxHealth: 20,
                        maxSP: 10,
                        physic: 5,
                        defense: 3,
                        magic: 1,
                        speed: 15),
                    abilities: newAbilitis
                    );
            var groupTemplate = new List<UnitProfile>();
            groupTemplate.Add(newProfile);

            return new Group(
                id: new GroupId("Slime"),
                template: new Template("Slime", groupTemplate),
                movementPattern: MovementPattern.Chase,
                aggroRange: new AggroRange(250f),
                speed: new MovementSpeed(6f),
                startPosition: new WorldPosition(600, 400)
            );
        }

        public static Group CreateGroupPlayer()
        {

            var ability = Ability.Create(
                        id: "base_attack",
                        name: "BaseAttack",
                        description: "Simple attack",
                        range: TargetRange.Melee,
                        target: Target.Enemy,
                        scales: new List<Scale>(),
                        costs: new List<Cost>(),
                        statusEffects: new List<Status>()
                        );

            var newAbilitis = new List<Ability>();
            newAbilitis.Add(ability);

            var newProfile = UnitProfile.Create(
                    name: "Player",
                    slot: 1,
                    stats: UnitStats.Create(
                        maxHealth: 20,
                        maxSP: 10,
                        physic: 5,
                        defense: 3,
                        magic: 1,
                        speed: 15),
                    abilities: newAbilitis
                    );
            var groupTemplate = new List<UnitProfile>();

            groupTemplate.Add(newProfile);
            groupTemplate.Add(newProfile with { SlotIndex = 5 });

            return new Group(
                id: new GroupId("Player"),
            template: new Template("Player", groupTemplate),
            movementPattern: MovementPattern.PlayerControlled,
            aggroRange: new AggroRange(0f),
            speed: new MovementSpeed(200f),
            startPosition: new WorldPosition(200, 200)
            );
        }
    }
}
