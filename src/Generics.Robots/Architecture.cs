using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Generics.Robots
{
    public interface IRobotAI<out TCommand> where TCommand : IMoveCommand
    {
        TCommand GetCommand();
    }

    public abstract class RobotAI<TCommand>: IRobotAI<TCommand> where TCommand : IMoveCommand
    {
        public abstract TCommand GetCommand();
    }

    public class ShooterAI : RobotAI<IShooterMoveCommand>
    {
        int counter = 1;

        public override IShooterMoveCommand GetCommand()
        {
            return ShooterCommand.ForCounter(counter++);
        }
    }

    public class BuilderAI : RobotAI<IMoveCommand>
    {
        int counter = 1;

        public override IMoveCommand GetCommand()
        {
            return BuilderCommand.ForCounter(counter++);
        }
    }

    public interface IDevice<in TCommand> where TCommand : IMoveCommand
    {
        string ExecuteCommand(TCommand command);
    }

    public abstract class Device<TCommand>: IDevice<TCommand> where TCommand : IMoveCommand
    {
        public abstract string ExecuteCommand(TCommand command);
    }

    public class Mover : Device<IMoveCommand>
    {
        public override string ExecuteCommand(IMoveCommand command)
        {
            if (command == null) throw new ArgumentException();

            return $"MOV {command.Destination.X}, {command.Destination.Y}";
        }
    }

    public class ShooterMover : Device<IShooterMoveCommand>
    {
        public override string ExecuteCommand(IShooterMoveCommand command)
        {
            if (command == null) throw new ArgumentException();

            var hide = command.ShouldHide ? "YES" : "NO";

            return $"MOV {command.Destination.X}, {command.Destination.Y}, USE COVER {hide}";
        }
    }

    public class Robot<TCommand> where TCommand : IMoveCommand
    {
        private readonly IRobotAI<TCommand> ai;
        private readonly IDevice<TCommand> device;

        public Robot(IRobotAI<TCommand> ai, IDevice<TCommand> executor)
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
    }

    public class Robot
    {
        public static Robot<TCommand> Create<TCommand>(IRobotAI<TCommand> ai, IDevice<TCommand> executor) where TCommand : IMoveCommand
        {
            return new Robot<TCommand>(ai, executor);
        }
    }
}
