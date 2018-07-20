﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Salar.Bois.Serializers
{
	public static class PrimitiveWriter
	{
		internal static void WriteNullableType(BinaryWriter writer, bool isnull)
		{
			writer.Write(isnull ? (byte)1 : (byte)0);
		}

		/// <summary>
		/// String - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, string str, Encoding encoding)
		{
			if (str == null)
			{
				WriteNullableType(writer, true);
			}
			else if (str.Length == 0)
			{
				PrimitivesConvertion.WriteVarInt(writer, (int?)0);
			}
			else
			{
				var strBytes = encoding.GetBytes(str);
				// Int32
				PrimitivesConvertion.WriteVarInt(writer, (int?)strBytes.Length);
				writer.Write(strBytes);
			}
		}

		/// <summary>
		/// char - Format: (Embedded-0-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, char c)
		{
			writer.Write((ushort)c);
		}

		/// <summary>
		/// char? - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, char? c)
		{
			PrimitivesConvertion.WriteVarInt(writer, (ushort?)c);
		}

		/// <summary>
		/// bool - Format: (Embedded=true-0-0-0-0-0-0-0)
		/// Embeddable range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, bool b)
		{
			writer.Write(b);
		}

		/// <summary>
		/// bool? - Format: (Embedded=true-Nullable-0-0-0-0-0-0)
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, bool? b)
		{
			byte? val = null;
			if (b.HasValue)
				val = b.Value ? (byte)1 : (byte)0;

			PrimitivesConvertion.WriteVarInt(writer, val);
		}

		/// <summary>
		/// DateTime - Format: (Kind:0-0-0-0-0-0-0-0) (dateTimeTicks:Embedded-0-0-0-0-0-0-0)[if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable kind range: always embeded
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, DateTime dateTime)
		{
			var kind = (byte)dateTime.Kind;

			if (dateTime == DateTime.MinValue)
			{
				writer.Write(kind);
				// min datetime indicator
				PrimitivesConvertion.WriteVarInt(writer, 0L);
			}
			else if (dateTime == DateTime.MaxValue)
			{
				writer.Write(kind);
				// max datetime indicator
				PrimitivesConvertion.WriteVarInt(writer, 1L);
			}
			else
			{
				writer.Write(kind);
				//Int64
				PrimitivesConvertion.WriteVarInt(writer, dateTime.Ticks);
			}
		}

		/// <summary>
		/// DateTime? - Format: (Kind:Nullable-0-0-0-0-0-0-0) (dateTimeTicks:Embedded-0-0-0-0-0-0-0)[if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable kind range: always embeded
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, DateTime? dt)
		{
			if (dt == null)
			{
				WriteNullableType(writer, true);
				return;
			}
			var dateTime = dt.Value;
			var kind = (byte?)dateTime.Kind;

			if (dateTime == DateTime.MinValue)
			{
				PrimitivesConvertion.WriteVarInt(writer, kind);
				// min datetime indicator
				PrimitivesConvertion.WriteVarInt(writer, 0L);
			}
			else if (dateTime == DateTime.MaxValue)
			{
				PrimitivesConvertion.WriteVarInt(writer, kind);
				// max datetime indicator
				PrimitivesConvertion.WriteVarInt(writer, 1L);
			}
			else
			{
				PrimitivesConvertion.WriteVarInt(writer, kind);
				//Int64
				PrimitivesConvertion.WriteVarInt(writer, dateTime.Ticks);
			}
		}

		/// <summary>
		/// DateTimeOffset - Format: (Offset:Embedded-0-0-0-0-0-0-0)[if ofset not embedded?0-0-0-0-0-0-0-0] (dateTimeOffsetTicks:Embedded-0-0-0-0-0-0-0)[if ticks not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable offset range: 0..127
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, DateTimeOffset dateTimeOffset)
		{
			var offset = dateTimeOffset.Offset;
			short offsetMinutes;
			unchecked
			{
				offsetMinutes = (short)((offset.Hours * 60) + offset.Minutes);
			}
			// int16
			PrimitivesConvertion.WriteVarInt(writer, offsetMinutes);

			// int64
			PrimitivesConvertion.WriteVarInt(writer, dateTimeOffset.Ticks);
		}

		/// <summary>
		/// DateTimeOffset? - Format: (Offset:Embedded-Nullable-0-0-0-0-0-0)[if ofset not embedded?0-0-0-0-0-0-0-0] (dateTimeOffsetTicks:Embedded-0-0-0-0-0-0-0)[if ticks not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable offset range: 0..63
		/// Embeddable ticks range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, DateTimeOffset? dto)
		{
			if (dto == null)
			{
				WriteNullableType(writer, true);
				return;
			}
			var dateTimeOffset = dto.Value;

			var offset = dateTimeOffset.Offset;
			short? offsetMinutes;
			unchecked
			{
				offsetMinutes = (short)((offset.Hours * 60) + offset.Minutes);
			}
			// int16
			PrimitivesConvertion.WriteVarInt(writer, offsetMinutes);

			// int64
			PrimitivesConvertion.WriteVarInt(writer, dateTimeOffset.Ticks);
		}

		/// <summary>
		/// byte[] - Format: (Array Length:Embedded-Nullable-0-0-0-0-0-0) [if array length not embedded?0-0-0-0-0-0-0-0] (data:0-0-0-0-0-0-0-0)
		/// Embeddable Array Length range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, byte[] bytes)
		{
			// uint doesn't deal with negative numbers
			PrimitivesConvertion.WriteVarInt(writer, (uint?)bytes.Length);
			writer.Write(bytes);
		}

		/// <summary>
		/// String - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, Enum e)
		{
			if (e == null)
			{
				WriteNullableType(writer, true);
				return;
			}
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, (int?)((object)e));
		}


		/// <summary>
		/// TimeSpan - Format: (Embedded-0-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, TimeSpan timeSpan)
		{
			PrimitivesConvertion.WriteVarInt(writer, timeSpan.Ticks);
		}

		/// <summary>
		/// TimeSpan? - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, TimeSpan? timeSpan)
		{
			if (timeSpan == null)
			{
				WriteNullableType(writer, true);
				return;
			}
			PrimitivesConvertion.WriteVarInt(writer, (long?)timeSpan.Value.Ticks);
		}

		/// <summary>
		/// Version - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, Version version)
		{
			if (version == null)
			{
				WriteNullableType(writer, true);
				return;
			}
			WriteValue(writer, version.ToString(), Encoding.Default);
		}

		/// <summary>
		/// Guid - Format: (Embedded-0-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..127
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, Guid guid)
		{
			if (guid == Guid.Empty)
			{
				// Int32
				PrimitivesConvertion.WriteVarInt(writer, 0);
				return;
			}

			var data = guid.ToByteArray();

			// Int32
			PrimitivesConvertion.WriteVarInt(writer, (uint)data.Length);
			writer.Write(data);
		}

		/// <summary>
		/// Guid? - Format: (Embedded-Nullable-0-0-0-0-0-0) [if not embedded?0-0-0-0-0-0-0-0]
		/// Embeddable range: 0..63
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, Guid? g)
		{
			if (g == null)
			{
				WriteNullableType(writer, true);
				return;
			}

			var guid = g.Value;
			if (guid == Guid.Empty)
			{
				// Int32
				PrimitivesConvertion.WriteVarInt(writer, 0);
				return;
			}

			var data = guid.ToByteArray();

			// Int32
			PrimitivesConvertion.WriteVarInt(writer, (uint?)data.Length);
			writer.Write(data);
		}

		/// <summary>
		/// DBNull? - Format: (Embedded=true-Nullable=true-0-0-0-0-0-0)
		/// </summary>
		internal static void WriteValue(BinaryWriter writer, DBNull dbNull)
		{
			WriteNullableType(writer, true);
		}



	}
}