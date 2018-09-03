using System.Reactive.Disposables;

namespace kenzauros.RHarbor.ViewModels
{
    /// <summary>
    /// <see cref="CompositeDisposable"/> をもつ ViewModel の基底クラスを表します。
    /// </summary>
    public abstract class CompositeDisposableViewModelBase
    {
        /// <summary>
        /// このオブジェクト内で破棄すべきオブジェクトを保持します。
        /// </summary>
        public CompositeDisposable Disposable { get; } = new CompositeDisposable();

        /// <summary>
        /// <see cref="Disposable"/> に含まれる全てのリソースを破棄します。
        /// </summary>
        public virtual void Dispose()
        {
            if (Disposable?.IsDisposed == false)
            {
                Disposable.Dispose();
            }
        }
    }
}
