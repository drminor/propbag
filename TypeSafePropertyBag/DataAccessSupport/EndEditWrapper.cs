
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DRM.TypeSafePropertyBag.DataAccessSupport
{
    public class EndEditWrapper<T> : ObservableCollection<T>, INotifyItemEndEdit where T: INotifyItemEndEdit
    {
        public EndEditWrapper()
        {
        }

        public EndEditWrapper(List<T> list)
        {
            // TODO: See if the base implementation does the same thing.
            foreach(T item in list)
            {
                // This calls insert and thereby we attach our handler to each item's ItemEndEdit event.
                Add(item);
            }
        }

        public EndEditWrapper(IEnumerable<T> collection) //: base(collection)
        {
            //TODO: Verify that the base implementation does not call Add for each item.
            // Perhaps we can attach our handler to each item after calling base(collection)
            foreach (T item in collection)
            {
                // This calls insert and thereby we attach our handler to each item's ItemEndEdit event.
                Add(item);
            }
        }

        public event EventHandler<EventArgs> ItemEndEdit;

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            // handle any EndEdit events relating to this item
            item.ItemEndEdit += ItemEndEditHandler;
        }

        void ItemEndEditHandler(object sender, EventArgs e)
        {
            // simply forward any EndEdit events
            ItemEndEdit?.Invoke(sender, e);
        }

    }
}
