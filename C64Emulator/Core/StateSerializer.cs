using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace C64Emulator.Core;

/// <summary>
/// Writes and reads the primitive and one-dimensional array fields of a
/// component. Snapshot code uses this for chip state so adding a timer or
/// latch does not also require a hand-written serialization line. Reference
/// fields are handled explicitly by <see cref="MachineState"/>.
/// </summary>
internal static class StateSerializer
{
    private static readonly ConcurrentDictionary<Type, FieldInfo[]> FieldCache = new();

    public static void WriteFields(
        BinaryWriter writer,
        object target,
        params string[] skip)
    {
        var skipped = new HashSet<string>(skip, StringComparer.Ordinal);

        foreach (FieldInfo field in Fields(target.GetType()))
        {
            if (
                field.GetCustomAttribute<NonSerializedAttribute>() is not null ||
                skipped.Contains(field.Name) ||
                !IsSupported(field.FieldType))
            {
                continue;
            }

            WriteValue(writer, field.GetValue(target), field.FieldType);
        }
    }

    public static void ReadFields(
        BinaryReader reader,
        object target,
        params string[] skip)
    {
        var skipped = new HashSet<string>(skip, StringComparer.Ordinal);

        foreach (FieldInfo field in Fields(target.GetType()))
        {
            if (
                field.GetCustomAttribute<NonSerializedAttribute>() is not null ||
                skipped.Contains(field.Name) ||
                !IsSupported(field.FieldType))
            {
                continue;
            }

            object? value = ReadValue(reader, field.FieldType);

            // Arrays such as Memory.ram and ColorRam.values are readonly.
            // Copy into the existing instance instead of replacing it.
            if (
                field.IsInitOnly &&
                value is Array source &&
                field.GetValue(target) is Array destination &&
                destination.Length == source.Length)
            {
                Array.Copy(source, destination, source.Length);
                continue;
            }

            field.SetValue(target, value);
        }
    }

    public static void WriteString(BinaryWriter writer, string? value)
    {
        writer.Write(value is not null);

        if (value is not null)
            writer.Write(value);
    }

    public static string? ReadString(BinaryReader reader)
    {
        return reader.ReadBoolean() ? reader.ReadString() : null;
    }

    private static FieldInfo[] Fields(Type type)
    {
        return FieldCache.GetOrAdd(type, static declaredType =>
        {
            var hierarchy = new Stack<Type>();

            for (
                Type? current = declaredType;
                current is not null && current != typeof(object);
                current = current.BaseType)
            {
                hierarchy.Push(current);
            }

            List<FieldInfo> fields = [];

            while (hierarchy.Count > 0)
            {
                Type current = hierarchy.Pop();

                fields.AddRange(
                    current
                        .GetFields(
                            BindingFlags.Instance |
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.DeclaredOnly)
                        .OrderBy(field => field.MetadataToken));
            }

            return [.. fields];
        });
    }

    private static bool IsSupported(Type type)
    {
        if (type.IsPrimitive || type.IsEnum || type == typeof(string))
            return true;

        if (type.IsArray && type.GetArrayRank() == 1)
        {
            Type? element = type.GetElementType();

            return
                element is not null &&
                (element.IsPrimitive || element.IsEnum || element == typeof(string));
        }

        return false;
    }

    private static void WriteValue(
        BinaryWriter writer,
        object? value,
        Type type)
    {
        if (type == typeof(bool))
            writer.Write((bool)value!);
        else if (type == typeof(byte))
            writer.Write((byte)value!);
        else if (type == typeof(sbyte))
            writer.Write((sbyte)value!);
        else if (type == typeof(short))
            writer.Write((short)value!);
        else if (type == typeof(ushort))
            writer.Write((ushort)value!);
        else if (type == typeof(int))
            writer.Write((int)value!);
        else if (type == typeof(uint))
            writer.Write((uint)value!);
        else if (type == typeof(long))
            writer.Write((long)value!);
        else if (type == typeof(ulong))
            writer.Write((ulong)value!);
        else if (type == typeof(float))
            writer.Write((float)value!);
        else if (type == typeof(double))
            writer.Write((double)value!);
        else if (type == typeof(char))
            writer.Write((char)value!);
        else if (type == typeof(string))
            WriteString(writer, (string?)value);
        else if (type.IsEnum)
            writer.Write(Convert.ToInt64(value));
        else if (type.IsArray)
        {
            var array = (Array)value!;
            writer.Write(array.Length);

            if (type.GetElementType() == typeof(byte))
            {
                writer.Write((byte[])array);
                return;
            }

            Type elementType = type.GetElementType()!;

            foreach (object? item in array)
                WriteValue(writer, item, elementType);
        }
        else
        {
            throw new NotSupportedException(
                "Cannot snapshot field type " + type.FullName + ".");
        }
    }

    private static object? ReadValue(BinaryReader reader, Type type)
    {
        if (type == typeof(bool))
            return reader.ReadBoolean();

        if (type == typeof(byte))
            return reader.ReadByte();

        if (type == typeof(sbyte))
            return reader.ReadSByte();

        if (type == typeof(short))
            return reader.ReadInt16();

        if (type == typeof(ushort))
            return reader.ReadUInt16();

        if (type == typeof(int))
            return reader.ReadInt32();

        if (type == typeof(uint))
            return reader.ReadUInt32();

        if (type == typeof(long))
            return reader.ReadInt64();

        if (type == typeof(ulong))
            return reader.ReadUInt64();

        if (type == typeof(float))
            return reader.ReadSingle();

        if (type == typeof(double))
            return reader.ReadDouble();

        if (type == typeof(char))
            return reader.ReadChar();

        if (type == typeof(string))
            return ReadString(reader);

        if (type.IsEnum)
            return Enum.ToObject(type, reader.ReadInt64());

        if (type.IsArray && type.GetArrayRank() == 1)
        {
            int length = reader.ReadInt32();

            if (length < 0 || length > 16_000_000)
                throw new InvalidDataException("Invalid array length in state.");

            Type elementType = type.GetElementType()!;

            if (elementType == typeof(byte))
            {
                byte[] bytes = reader.ReadBytes(length);

                if (bytes.Length != length)
                    throw new EndOfStreamException();

                return bytes;
            }

            Array array = Array.CreateInstance(elementType, length);

            for (var i = 0; i < length; i++)
                array.SetValue(ReadValue(reader, elementType), i);

            return array;
        }

        throw new NotSupportedException(
            "Cannot read snapshot field type " + type.FullName + ".");
    }
}
