﻿namespace GDShrapt.Reader
{
    public sealed class GDDictionaryKeyValueDeclaration : GDNode,
        ITokenOrSkipReceiver<GDExpression>,
        ITokenOrSkipReceiver<GDColon>,
        ITokenOrSkipReceiver<GDAssign>,
        ITokenReceiver<GDNewLine>,
        INewLineReceiver
    {
        bool _checkedColon;

        public GDExpression Key
        {
            get => _form.Token0;
            set => _form.Token0 = value;
        }
        public GDColon Colon
        {
            get => _form.Token1 as GDColon;
            set => _form.Token1 = value;
        }
        public GDAssign Assign
        {
            get => _form.Token1 as GDAssign;
            set => _form.Token1 = value;
        }
        public GDExpression Value
        {
            get => _form.Token2;
            set => _form.Token2 = value;
        }

        public enum State
        {
            Key,
            ColonOrAssign,
            Value,
            Completed
        }

        readonly GDTokensForm<State, GDExpression, GDPairToken, GDExpression> _form;
        readonly int _intendation;

        internal GDDictionaryKeyValueDeclaration(int intendation)
        {
            _form = new GDTokensForm<State, GDExpression, GDPairToken, GDExpression>(this);
            _intendation = intendation;
        }

        public GDDictionaryKeyValueDeclaration()
        {
            _form = new GDTokensForm<State, GDExpression, GDPairToken, GDExpression>(this);
        }

        public override GDTokensForm Form => _form;
        public GDTokensForm<State, GDExpression, GDPairToken, GDExpression> TypedForm => _form;
        internal override void HandleChar(char c, GDReadingState state)
        {
            if (this.ResolveSpaceToken(c, state))
                return;

            switch (_form.State)
            {
                case State.Key:
                    this.ResolveExpression(c, state, _intendation, this, allowAssignment: false);
                    break;
                case State.ColonOrAssign:
                    if (!_checkedColon)
                        this.ResolveColon(c, state);
                    else
                        this.ResolveAssign(c, state);
                    break;
                case State.Value:
                    this.ResolveExpression(c, state, _intendation, this);
                    break;
                default:
                    state.PopAndPass(c);
                    break;
            }
        }

        internal override void HandleNewLineChar(GDReadingState state)
        {
            if (_form.State == State.Completed)
                state.PopAndPassNewLine();
            else
                _form.AddBeforeActiveToken(new GDNewLine());
        }

        public override GDNode CreateEmptyInstance()
        {
            return new GDDictionaryKeyValueDeclaration();
        }

        internal override void Visit(IGDVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal override void Left(IGDVisitor visitor)
        {
            visitor.Left(this);
        }

        void ITokenReceiver<GDExpression>.HandleReceivedToken(GDExpression token)
        {
            if (_form.IsOrLowerState(State.Key))
            {
                _form.State = State.ColonOrAssign;
                Key = token;
                return;
            }

            if (_form.IsOrLowerState(State.Value))
            {
                _form.State = State.Completed;
                Value = token;
                return;
            }

            throw new GDInvalidStateException();
        }

        void ITokenSkipReceiver<GDExpression>.HandleReceivedTokenSkip()
        {
            if (_form.IsOrLowerState(State.Key))
            {
                _form.State = State.ColonOrAssign;
                return;
            }

            if (_form.IsOrLowerState(State.Value))
            {
                _form.State = State.Completed;
                return;
            }

            throw new GDInvalidStateException();
        }

        void ITokenReceiver<GDColon>.HandleReceivedToken(GDColon token)
        {
            if (_form.IsOrLowerState(State.ColonOrAssign))
            {
                _checkedColon = true;
                _form.State = State.Value;
                Colon = token;
                return;
            }

            throw new GDInvalidStateException();
        }

        void ITokenSkipReceiver<GDColon>.HandleReceivedTokenSkip()
        {
            if (_form.IsOrLowerState(State.ColonOrAssign))
            {
                _checkedColon = true;
                return;
            }

            throw new GDInvalidStateException();
        }

        void ITokenReceiver<GDAssign>.HandleReceivedToken(GDAssign token)
        {
            if (_form.IsOrLowerState(State.ColonOrAssign))
            {
                _form.State = State.Value;
                Assign = token;
                return;
            }

            throw new GDInvalidStateException();
        }

        void ITokenSkipReceiver<GDAssign>.HandleReceivedTokenSkip()
        {
            if (_form.IsOrLowerState(State.ColonOrAssign))
            {
                _form.State = State.Value;
                return;
            }

            throw new GDInvalidStateException();
        }

        void ITokenReceiver<GDNewLine>.HandleReceivedToken(GDNewLine token)
        {
            if (_form.State != State.Completed)
            {
                _form.AddBeforeActiveToken(token);
                return;
            }

            throw new GDInvalidStateException();
        }

        void INewLineReceiver.HandleReceivedToken(GDNewLine token)
        {
            if (_form.State != State.Completed)
            {
                _form.AddBeforeActiveToken(token);
                return;
            }

            throw new GDInvalidStateException();
        }
    }
}
