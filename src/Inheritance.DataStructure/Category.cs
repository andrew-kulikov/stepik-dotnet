using System;

namespace Inheritance.DataStructure
{
    public class Category : IComparable
    {
        private readonly MessageTopic _messageTopic;
        private readonly MessageType _messageType;
        private readonly string _productName;

        public Category(string productName, MessageType messageType, MessageTopic messageTopic)
        {
            _productName = productName;
            _messageType = messageType;
            _messageTopic = messageTopic;
        }

        public int CompareTo(object other)
        {
            if (other == null) return 1;

            var category = other as Category;
            if (category == null) return 1;

            var nameComparisonResult = string.Compare(_productName, category._productName, StringComparison.Ordinal);
            if (nameComparisonResult != 0) return nameComparisonResult;

            if (_messageType > category._messageType) return 1;
            if (_messageType < category._messageType) return -1;

            if (_messageTopic > category._messageTopic) return 1;
            if (_messageTopic < category._messageTopic) return -1;

            return 0;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            var category = obj as Category;
            if (category == null) return false;

            return category.Equals(this);
        }

        protected bool Equals(Category other)
        {
            return _messageTopic == other._messageTopic && _messageType == other._messageType &&
                   _productName == other._productName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) _messageTopic;
                hashCode = (hashCode * 397) ^ (int) _messageType;
                hashCode = (hashCode * 397) ^ (_productName != null ? _productName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{_productName}.{_messageType}.{_messageTopic}";
        }

        public static bool operator ==(Category c1, Category c2)
        {
            if (c1 is null && c2 is null) return true;
            if (c1 is null || c2 is null) return false;

            return c1.Equals(c2);
        }

        public static bool operator !=(Category c1, Category c2)
        {
            return !(c1 == c2);
        }

        public static bool operator >(Category c1, Category c2)
        {
            return c1.CompareTo(c2) > 0;
        }

        public static bool operator <(Category c1, Category c2)
        {
            return c1.CompareTo(c2) < 0;
        }

        public static bool operator >=(Category c1, Category c2)
        {
            return c1 == c2 || c1 > c2;
        }

        public static bool operator <=(Category c1, Category c2)
        {
            return c1 == c2 || c1 < c2;
        }
    }
}