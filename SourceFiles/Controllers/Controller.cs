using System;
using FarseerPhysics.Dynamics;

namespace FarseerPhysics.Controllers
{
    [Flags]
    public enum ControllerType
    {
        GravityController = (1 << 0),
        VelocityLimitController = (1 << 1),
        AbstractForceController = (1 << 2)
    }

    public abstract class FilterData
    {
        public CollisionCategory DisabledOnCategories = CollisionCategory.None;
        public CollisionCategory EnabledOnCategories = CollisionCategory.All;

        public int DisabledOnGroup = 0;
        public int EnabledOnGroup = 0;

        public virtual bool IsActiveOn(Body body)
        {
            foreach (Fixture fixture in body.FixtureList)
            {
                //Disable
                if ((fixture.CollisionGroup == DisabledOnGroup) && fixture.CollisionGroup != 0 && DisabledOnGroup != 0)
                    return false;

                if ((fixture.CollisionCategories & DisabledOnCategories) != CollisionCategory.None)
                    return false;

                if (EnabledOnGroup != 0 || EnabledOnCategories != CollisionCategory.All)
                {
                    //Enable
                    if ((fixture.CollisionGroup == EnabledOnGroup) && fixture.CollisionGroup != 0 && EnabledOnGroup != 0)
                        return true;

                    if ((fixture.CollisionCategories & EnabledOnCategories) != CollisionCategory.None && EnabledOnCategories != CollisionCategory.All)
                        return true;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class FilterControllerData : FilterData
    {
        private ControllerType _type;

        public FilterControllerData(ControllerType type)
        {
            _type = type;
        }

        public override bool IsActiveOn(Body body)
        {
            if (body.ControllerFilter.IsControllerIgnored(_type))
                return false;

            return base.IsActiveOn(body);
        }
    }

    public class ControllerFilter
    {
        public ControllerType ControllerIgnores;

        /// <summary>
        /// Ignores the controller. The controller has no effect on this body.
        /// </summary>
        /// <param name="controller">The flags.</param>
        public void IgnoreController(ControllerType controller)
        {
            ControllerIgnores |= controller;
        }

        /// <summary>
        /// Restore the controller. The controller affects this body.
        /// </summary>
        /// <param name="controller">The flags.</param>
        public void RestoreController(ControllerType controller)
        {
            ControllerIgnores &= ~controller;
        }

        /// <summary>
        /// Determines whether this body ignores the the specified controller.
        /// </summary>
        /// <param name="controller">The flags.</param>
        /// <returns>
        /// 	<c>true</c> if the body has the specified flag; otherwise, <c>false</c>.
        /// </returns>
        public bool IsControllerIgnored(ControllerType controller)
        {
            return (ControllerIgnores & controller) == controller;
        }
    }

    public abstract class Controller
    {
        public FilterControllerData FilterData;

        public bool Enabled;

        public World World;

        public Controller(ControllerType controllerType)
        {
            FilterData = new FilterControllerData(controllerType);
        }

        public abstract void Update(float dt);
    }
}