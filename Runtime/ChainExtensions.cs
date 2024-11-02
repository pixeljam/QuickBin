using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickBin.ChainExtensions {
	public static class ChainExtensions {
		/// <summary>Executes an action.</summary>
		/// <param name="action">The action to execute.</param>
		/// <returns><c>@this</c></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TChain Then<TChain>(this TChain @this, Action action) {
			action();
			return @this;
		}
		
		/// <summary>Assigns a value to the out parameter.</summary>
		/// <param name="value">The value to assign.</param>
		/// <param name="variable">The variable to assign to.</param>
		/// <returns><c>@this</c></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TChain Assign<TChain, T>(this TChain @this, T value, out T variable) {
			variable = value;
			return @this;
		}
		
		/// <summary>Returns an unrelated object.</summary>
		/// <returns><c>value</c></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Output<T>(this object _, T value) => value;
		
		/// <summary>Executes an action if a condition is met.</summary>
		/// <param name="condition">The condition to check.</param>
		/// <param name="action">The action to execute.</param>
		/// <returns><c>@this</c></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TChain When<TChain>(this TChain @this, bool condition, Action<TChain> action) {
			if (condition) action(@this);
			return @this;
		}
		
		/// <summary>Executes an action for each value in the specified IEnumerable.</summary>
		/// <param name="values">The IEnumerable of values to act on.</param>
		/// <param name="action">The action to execute on each value.</param>
		/// <returns><c>@this</c></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TChain ForEach<TChain, T>(this TChain @this, IEnumerable<T> values, Action<T> action) {
			foreach (var value in values)
				action(value);
			return @this;
		}
		
		/// <summary>Executes an action for each value in the specified IEnumerable.</summary>
		/// <param name="values">The IEnumerable of values to act on.</param>
		/// <param name="action">The action to execute on each value.</param>
		/// <returns><c>@this</c></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TChain ForEach<TChain, T>(this TChain @this, IEnumerable<T> values, Action<TChain, T> action) {
			foreach (var value in values)
				action(@this, value);
			return @this;
		}
		
		/// <summary>Executes evaluates a function a specified number of times, returning the produced values as an IEnumerable.</summary>
		/// <param name="produced">An IEnumerable of all values returned by <c>func</c>.</param>
		/// <param name="func">The function to evaluate. Receives <c>@this</c> as an argument, returns a value of type <c>T</c>.</param>
		/// <param name="count">The number of times to evaluate the function.</param>
		/// <returns><c>@this</c></returns>
		public static TChain ForEach<TChain, T>(this TChain @this, out IEnumerable<T> produced, Func<TChain,T> func, int count) {
			IEnumerable<T> Iterate() {
				for (var i = 0; i < count; i++)
					yield return func(@this);
			}
			produced = Iterate();
			return @this;
		}
	}
}