﻿using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using System.Collections.Generic;

namespace Bridge.NET
{
    public class AssignmentBlock : AbstractEmitterBlock
    {
        public AssignmentBlock(Emitter emitter, AssignmentExpression assignmentExpression)
        {
            this.Emitter = emitter;
            this.AssignmentExpression = assignmentExpression;
        }

        public AssignmentExpression AssignmentExpression 
        { 
            get; 
            set; 
        }

        public override void Emit()
        {
            this.VisitAssignmentExpression();
        }

        protected void VisitAssignmentExpression()
        {
            AssignmentExpression assignmentExpression = this.AssignmentExpression;

            var delegateAssigment = false;
            bool isEvent = false;
            var initCount = this.Emitter.Writers.Count;

            if (assignmentExpression.Operator == AssignmentOperatorType.Add ||
                assignmentExpression.Operator == AssignmentOperatorType.Subtract)
            {
                var leftResolverResult = this.Emitter.Resolver.ResolveNode(assignmentExpression.Left, this.Emitter);
                var rightResolverResult = this.Emitter.Resolver.ResolveNode(assignmentExpression.Right, this.Emitter);
                var add = assignmentExpression.Operator == AssignmentOperatorType.Add;

                if (this.Emitter.Validator.IsDelegateOrLambda(leftResolverResult))
                {
                    delegateAssigment = true;
                    var leftMemberResolveResult = leftResolverResult as MemberResolveResult;
                    if (leftMemberResolveResult != null)
                    {
                        isEvent = leftMemberResolveResult.Member is DefaultResolvedEvent;
                    }

                    if (!isEvent)
                    {
                        this.Emitter.IsAssignment = true;
                        assignmentExpression.Left.AcceptVisitor(this.Emitter);
                        this.Emitter.IsAssignment = false;
                        this.Write(" = ");
                        this.Write(Emitter.ROOT + "." + (add ? Emitter.DELEGATE_COMBINE : Emitter.DELEGATE_REMOVE));
                        this.WriteOpenParentheses();
                    }                    
                }
            }

            this.Emitter.IsAssignment = true;
            this.Emitter.AssignmentType = assignmentExpression.Operator;
            assignmentExpression.Left.AcceptVisitor(this.Emitter);
            this.Emitter.AssignmentType = AssignmentOperatorType.Any;
            this.Emitter.IsAssignment = false;

            if (this.Emitter.Writers.Count == 0 && !delegateAssigment)
            {
                this.WriteSpace();
            }

            if (!delegateAssigment)
            {
                switch (assignmentExpression.Operator)
                {
                    case AssignmentOperatorType.Assign:
                        break;
                    case AssignmentOperatorType.Add:
                        this.Write("+");
                        break;
                    case AssignmentOperatorType.BitwiseAnd:
                        this.Write("&");
                        break;
                    case AssignmentOperatorType.BitwiseOr:
                        this.Write("|");
                        break;
                    case AssignmentOperatorType.Divide:
                        this.Write("/");
                        break;
                    case AssignmentOperatorType.ExclusiveOr:
                        this.Write("^");
                        break;
                    case AssignmentOperatorType.Modulus:
                        this.Write("%");
                        break;
                    case AssignmentOperatorType.Multiply:
                        this.Write("*");
                        break;
                    case AssignmentOperatorType.ShiftLeft:
                        this.Write("<<");
                        break;
                    case AssignmentOperatorType.ShiftRight:
                        this.Write(">>");
                        break;
                    case AssignmentOperatorType.Subtract:
                        this.Write("-");
                        break;
                    default:
                        throw this.Emitter.CreateException(assignmentExpression, "Unsupported assignment operator: " + assignmentExpression.Operator.ToString());
                }

                int count = this.Emitter.Writers.Count;

                if (count == 0)
                {
                    this.Write("= ");
                }
            }
            else if (!isEvent)
            {
                this.WriteComma();
            }

            assignmentExpression.Right.AcceptVisitor(this.Emitter);

            if (this.Emitter.Writers.Count > initCount)
            {
                this.PopWriter();
            }

            if (delegateAssigment)
            {
                this.WriteCloseParentheses();
            }
        }
    }
}
