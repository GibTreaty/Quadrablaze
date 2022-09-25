using UnityEngine;
using System;

namespace EasyLayoutNamespace {

    /// <summary>
    /// Grid constraints.
    /// </summary>
    public enum GridConstraints {
        /// <summary>
        /// Don't constraint the number of rows or columns.
        /// </summary>
        Flexible = 0,

        /// <summary>
        /// Constraint the number of columns to a specified number.
        /// </summary>
        FixedColumnCount = 1,

        /// <summary>
        /// Constraint the number of rows to a specified number.
        /// </summary>
        FixedRowCount = 2,
    }

    /// <summary>
    /// Compact constraints.
    /// </summary>
    public enum CompactConstraints {
        /// <summary>
        /// Don't constraint the number of rows or columns.
        /// </summary>
        Flexible = 0,

        /// <summary>
        /// Constraint the number of columns to a specified number.
        /// </summary>
        MaxColumnCount = 1,

        /// <summary>
        /// Constraint the number of rows to a specified number.
        /// </summary>
        MaxRowCount = 2,
    }

    /// <summary>
    /// Padding.
    /// </summary>
    [Serializable]
    public struct Padding {
        /// <summary>
        /// The left padding.
        /// </summary>
        [SerializeField]
        public float Left;

        /// <summary>
        /// The right padding.
        /// </summary>
        [SerializeField]
        public float Right;

        /// <summary>
        /// The top padding.
        /// </summary>
        [SerializeField]
        public float Top;

        /// <summary>
        /// The bottom padding.
        /// </summary>
        [SerializeField]
        public float Bottom;

        /// <summary>
        /// Initializes a new instance of the struct.
        /// </summary>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        /// <param name="top">Top.</param>
        /// <param name="bottom">Bottom.</param>
        public Padding(float left, float right, float top, float bottom) {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() {
            return String.Format("Padding(left: {0}, right: {1}, top: {2}, bottom: {3})",
                Left,
                Right,
                Top,
                Bottom
            );
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj) {
            if(obj is Padding) {
                return Equals((Padding)obj);
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(Padding other) {
            return Left == other.Left && Right == other.Right && Top == other.Top && Bottom == other.Bottom;
        }

        /// <summary>
        /// Hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() {
            return base.GetHashCode();
        }

        /// <summary>
        /// Compare specified paddings.
        /// </summary>
        /// <param name="padding1">First padding.</param>
        /// <param name="padding2">Second padding.</param>
        /// <returns>true if the paddings are equal; otherwise, false.</returns>
        public static bool operator ==(Padding padding1, Padding padding2) {
            return Equals(padding1, padding2);
        }

        /// <summary>
        /// Compare specified paddings.
        /// </summary>
        /// <param name="padding1">First padding.</param>
        /// <param name="padding2">Second padding.</param>
        /// <returns>true if the paddings not equal; otherwise, false.</returns>
        public static bool operator !=(Padding padding1, Padding padding2) {
            return !Equals(padding1, padding2);
        }
    }

    /// <summary>
    /// Children size.
    /// </summary>
    [Flags]
    public enum ChildrenSize {
        /// <summary>
        /// Don't change children sizes.
        /// </summary>
        DoNothing = 0,

        /// <summary>
        /// Set children sizes to preferred.
        /// </summary>
        SetPreferred = 1,

        /// <summary>
        /// Set children size to maximum size of children preferred.
        /// </summary>
        SetMaxFromPreferred = 2,

        /// <summary>
        /// Stretch children size to fit container.
        /// </summary>
        FitContainer = 3,

        /// <summary>
        /// Shrink children size if UI size more than RectTransform size.
        /// </summary>
        ShrinkOnOverflow = 4,
    }

    /// <summary>
    /// Anchors.
    /// </summary>
    [Flags]
    public enum Anchors {
        /// <summary>
        /// UpperLeft.
        /// </summary>
        UpperLeft = 0,

        /// <summary>
        /// UpperCenter.
        /// </summary>
        UpperCenter = 1,

        /// <summary>
        /// UpperRight.
        /// </summary>
        UpperRight = 2,

        /// <summary>
        /// MiddleLeft.
        /// </summary>
        MiddleLeft = 3,

        /// <summary>
        /// MiddleCenter.
        /// </summary>
        MiddleCenter = 4,

        /// <summary>
        /// MiddleRight.
        /// </summary>
        MiddleRight = 5,

        /// <summary>
        /// LowerLeft.
        /// </summary>
        LowerLeft = 6,

        /// <summary>
        /// LowerCenter.
        /// </summary>
        LowerCenter = 7,

        /// <summary>
        /// LowerRight.
        /// </summary>
        LowerRight = 8,
    }

    /// <summary>
    /// Stackings.
    /// </summary>
    [Flags]
    public enum Stackings {
        /// <summary>
        /// Horizontal.
        /// </summary>
        Horizontal = 0,

        /// <summary>
        /// Vertical.
        /// </summary>
        Vertical = 1,
    }

    /// <summary>
    /// Horizontal aligns.
    /// </summary>
    [Flags]
    public enum HorizontalAligns {
        /// <summary>
        /// Left.
        /// </summary>
        Left = 0,

        /// <summary>
        /// Center.
        /// </summary>
        Center = 1,

        /// <summary>
        /// Right.
        /// </summary>
        Right = 2,
    }

    /// <summary>
    /// Inner aligns.
    /// </summary>
    [Flags]
    public enum InnerAligns {
        /// <summary>
        /// Top.
        /// </summary>
        Top = 0,

        /// <summary>
        /// Middle.
        /// </summary>
        Middle = 1,

        /// <summary>
        /// Bottom.
        /// </summary>
        Bottom = 2,
    }

    /// <summary>
    /// Layout type to use.
    /// </summary>
    [Flags]
    public enum LayoutTypes {
        /// <summary>
        /// Compact.
        /// </summary>
        Compact = 0,

        /// <summary>
        /// Grid.
        /// </summary>
        Grid = 1,
    }
}