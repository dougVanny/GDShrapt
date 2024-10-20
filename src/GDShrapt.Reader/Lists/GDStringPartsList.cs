﻿namespace GDShrapt.Reader
{
    public class GDStringPartsList : GDSeparatedList<GDStringPart, GDMultiLineSplitToken>, 
        ITokenOrSkipReceiver<GDStringPart>,
        ITokenOrSkipReceiver<GDMultiLineSplitToken>
    {
        bool _firstSlashChecking;
        bool _ended;
        readonly GDStringBoundingChar _bounder;

        public GDStringPartsList()
        {
        }

        internal GDStringPartsList(GDStringBoundingChar bounder)
        {
            _bounder = bounder;
        }

        internal override void HandleChar(char c, GDReadingState state)
        {
            if (_ended)
            {
                state.PopAndPass(c);
                return;
            }

            this.ResolveStringPart(c, state, _bounder);
        }

        internal override void HandleNewLineChar(GDReadingState state)
        {
            _ended = true;
            state.PopAndPassNewLine();
        }

        internal override void HandleLeftSlashChar(GDReadingState state)
        {
            if (Count == 0)
            {
                _firstSlashChecking = true;
                this.ResolveStringPart('\\', state, _bounder);
                return;
            }

            _ended = false;
            ListForm.AddToEnd(state.Push(new GDMultiLineSplitToken()));
            state.PassLeftSlashChar();
        }

        internal override void HandleSharpChar(GDReadingState state)
        {
            HandleChar('#', state);
        }

        public override GDNode CreateEmptyInstance()
        {
            return new GDStringPartsList();
        }

        internal override void Left(IGDVisitor visitor)
        {
            visitor.Left(this);
        }

        internal override void Visit(IGDVisitor visitor)
        {
            visitor.Visit(this);
        }

        void ITokenReceiver<GDStringPart>.HandleReceivedToken(GDStringPart token)
        {
            ListForm.AddToEnd(token);
        }

        void ITokenSkipReceiver<GDStringPart>.HandleReceivedTokenSkip()
        {
            if (!_firstSlashChecking)
                _ended = true;
            else
                _firstSlashChecking = false;
        }

        void ITokenReceiver<GDMultiLineSplitToken>.HandleReceivedToken(GDMultiLineSplitToken token)
        {
            ListForm.AddToEnd(token);
        }

        void ITokenSkipReceiver<GDMultiLineSplitToken>.HandleReceivedTokenSkip()
        {
            _ended = true;
        }
    }
}
