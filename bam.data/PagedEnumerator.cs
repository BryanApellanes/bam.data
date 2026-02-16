/*
	Copyright © Bryan Apellanes 2015  
*/

namespace Bam.Data
{
    /// <summary>
    /// Abstract base class for enumerating paged results of type T.
    /// </summary>
    /// <typeparam name="T">The type of items to enumerate.</typeparam>
    public abstract class PagedEnumerator<T> : IEnumerator<T>
    {
        protected int currentItemIndex;
        protected int currentPageIndex;
        protected List<T> currentPage;

        public PagedEnumerator()
        {
            Reset();
        }

        /// <summary>
        /// When implemented by a derived class should set the 
        /// CurrentPage property to the next page.
        /// </summary>
        /// <returns></returns>
        public abstract bool MoveNextPage();

        /// <summary>
        /// Gets the list of items on the current page.
        /// </summary>
        public List<T> CurrentPage
        {
            get => this.currentPage;
            protected set => this.currentPage = value;
        }

        /// <summary>
        /// Represents the current index of the 
        /// current page.
        /// </summary>
        public int CurrentItemIndex
        {
            get => this.currentItemIndex;
            protected set => this.currentItemIndex = value;
        }

        /// <summary>
        /// Represents the index of the current page.
        /// </summary>
        public int CurrentPageIndex
        {
            get => this.currentPageIndex;
            protected set => this.currentPageIndex = value;
        }
        
        #region IEnumerator<T> Members

        /// <summary>
        /// Returns the item of the current page at 
        /// the current item index.
        /// </summary>
        public T Current => CurrentPage != null ? CurrentPage[CurrentItemIndex] : default(T);

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            this.Reset();
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current => Current;

        /// <summary>
        /// Advances to the next item, moving to the next page if needed.
        /// </summary>
        /// <returns>True if there is a next item; false if enumeration is complete.</returns>
        public bool MoveNext()
        {
            currentItemIndex++;
            if (currentItemIndex >= currentPage.Count)
            {
                currentItemIndex = 0;
                return MoveNextPage();
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Reset the current item and page back to the start
        /// </summary>
        public virtual void Reset()
        {
            currentItemIndex = -1;
            currentPageIndex = -1;
            currentPage = new List<T>();
        }
        #endregion
    }
}
