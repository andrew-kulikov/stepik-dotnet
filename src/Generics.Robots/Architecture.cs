using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Generics.Robots
{
    public abstract class RobotAI
    {
        public abstract object GetCommand();
    }

    public class ShooterAI : RobotAI
    {
        int counter = 1;

        public override object GetCommand()
        {
            return ShooterCommand.ForCounter(counter++);
        }
    }

    public class BuilderAI : RobotAI
    {
        int counter = 1;

        public override object GetCommand()
        {
            return BuilderCommand.ForCounter(counter++);
        }
    }

    public abstract class Device
    {
        public abstract string ExecuteCommand(object command);
    }

    public class Mover : Device
    {
        public override string ExecuteCommand(object _command)
        {
            var command = _command as IMoveCommand;
            if (command == null)
                throw new ArgumentException();
            return $"MOV {command.Destination.X}, {command.Destination.Y}";
        }
    }

    public class ShooterMover : Device
    {
        public override string ExecuteCommand(object _command)
        {
            var command = _command as IShooterMoveCommand;
            if (command == null)
                throw new ArgumentException();
            var hide = command.ShouldHide ? "YES" : "NO";
            return $"MOV {command.Destination.X}, {command.Destination.Y}, USE COVER {hide}";
        }
    }

    public class Robot
    {
        private readonly RobotAI ai;
        private readonly Device device;

        public Robot(RobotAI ai, Device executor)
        {
            this.ai = ai;
            this.device = executor;
        }

        public IEnumerable<string> Start(int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                var command = ai.GetCommand();
                if (command == null)
                    break;
                yield return device.ExecuteCommand(command);
            }
        }

        public static Robot Create<TCommand>(RobotAI ai, Device executor)
        {
            return new Robot(ai, executor);
        }
    }
}
