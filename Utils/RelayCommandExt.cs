using System;
using Playnite.SDK;

namespace BackToGame
{
    public class RelayCommandExt<D, T> : RelayCommandBase
    {
        private readonly Action<D, T> execute;
        private readonly D data;
        public RelayCommandExt(D Data, Action<D, T> Execute)
        {
            execute = Execute;
            data = Data;
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            if (parameter is T obj)
            {
                execute(data, obj);
            }
            else
            {
                execute(data, default(T));
            }
        }
    }
}
