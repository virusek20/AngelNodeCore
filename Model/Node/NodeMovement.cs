using System;
using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeMovement : ObservableObject, INode
    {
        public enum MovementTypeEnum
        {
            Enter,
            Exit
        };

        public enum MovementDirectionEnum
        {
            Left,
            Right,
            Center
        };

        public enum MovementAnimationEnum
        {
            Fade,
            Slide
        }

        private Character _character;
        private MovementTypeEnum _movementType;
        private MovementDirectionEnum _movementDirection;
        private MovementAnimationEnum _movementAnimation = MovementAnimationEnum.Fade;

        public Character Character
        {
            get => _character;
            set { Set(() => Character, ref _character, value); }
        }

        public MovementTypeEnum MovementType
        {
            get => _movementType;
            set { Set(() => MovementType, ref _movementType, value); }
        }

        public MovementDirectionEnum MovementDirection
        {
            get => _movementDirection;
            set { Set(() => MovementDirection, ref _movementDirection, value); }
        }
        public MovementAnimationEnum MovementAnimation
        {
            get => _movementAnimation;
            set { Set(() => MovementAnimation, ref _movementAnimation, value); }
        }
        public string MovementTypeString
        {
            get
            {
                switch (MovementType)
                {
                    case MovementTypeEnum.Enter:
                        return "MovementType::ENTER";
                    case MovementTypeEnum.Exit:
                        return "MovementType::EXIT";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public string MovementDirectionString
        {
            get
            {
                switch (MovementDirection)
                {
                    case MovementDirectionEnum.Left:
                        return "MovementDirection::LEFT";
                    case MovementDirectionEnum.Right:
                        return "MovementDirection::RIGHT";
                    case MovementDirectionEnum.Center:
                        return "MovementDirection::CENTER";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public string MovementAnimationString
        {
            get
            {
                switch (MovementAnimation)
                {
                    case MovementAnimationEnum.Fade:
                        return "FadeType::NONE";
                    case MovementAnimationEnum.Slide:
                        return "FadeType::FADING";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeMovement(this);
        }
    }
}
