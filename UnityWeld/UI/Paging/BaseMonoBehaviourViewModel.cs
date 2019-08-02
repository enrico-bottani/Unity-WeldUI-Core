using System.ComponentModel;
using UnityEngine;

namespace UnityWeld.UI.Paging
{
    public class BaseMonoBehaviourViewModel : MonoBehaviour, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}