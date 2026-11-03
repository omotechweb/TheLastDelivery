using System.Collections.Generic;
using TheLastDelivery.Core;

namespace TheLastDelivery.Items;

/// <summary>Envanterdeki tek bir yuva: aynı türden en fazla MaxStackPerSlot eşya istiflenir.</summary>
public sealed class InventorySlot
{
    public InventorySlot(ItemType type) => Type = type;

    public ItemType Type { get; }
    public int Count { get; internal set; }
}

/// <summary>
/// Arthur'un çantası: 4 yuva. Aynı türden eşyalar önce mevcut yuvada istiflenir,
/// yuva dolarsa boş bir yuvaya geçer. Boşalan yuva serbest kalır.
/// </summary>
public sealed class Inventory
{
    private readonly InventorySlot[] _slots = new InventorySlot[GameConfig.InventorySlots];

    /// <summary>Yuvalar; boş yuva null'dır.</summary>
    public IReadOnlyList<InventorySlot> Slots => _slots;

    public int UsedSlots
    {
        get
        {
            int used = 0;
            foreach (var slot in _slots)
                if (slot != null) used++;
            return used;
        }
    }

    /// <summary>Yolda toplanan yağmaların taşındığı kargo vagonu (satılmak için).</summary>
    public Economy.Cargo Cargo { get; } = new();

    /// <summary>Yolculara paylaştırılan erzak (yuvalardan bağımsız, ayrı bir çuvalda tutulur).</summary>
    public int Rations { get; private set; }

    public void Clear()
    {
        for (int i = 0; i < _slots.Length; i++)
            _slots[i] = null;
        Cargo.Clear();
        Rations = 0;
    }

    /// <returns>Çuvala sığan erzak sayısı (0 = çuval dolu).</returns>
    public int AddRations(int count)
    {
        int added = System.Math.Min(count, GameConfig.MaxRations - Rations);
        if (added <= 0) return 0;
        Rations += added;
        return added;
    }

    public bool TryConsumeRation()
    {
        if (Rations <= 0) return false;
        Rations--;
        return true;
    }

    /// <returns>Yer yoksa false (envanter dolu).</returns>
    public bool TryAdd(ItemType type)
    {
        // Önce aynı türden, dolmamış bir yuva ara
        foreach (var slot in _slots)
        {
            if (slot != null && slot.Type == type && slot.Count < GameConfig.MaxStackPerSlot)
            {
                slot.Count++;
                return true;
            }
        }

        // Yoksa ilk boş yuvaya yerleştir
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null)
            {
                _slots[i] = new InventorySlot(type) { Count = 1 };
                return true;
            }
        }
        return false;
    }

    public int CountOf(ItemType type)
    {
        int total = 0;
        foreach (var slot in _slots)
            if (slot != null && slot.Type == type) total += slot.Count;
        return total;
    }

    public bool Has(ItemType type) => CountOf(type) > 0;

    /// <summary>Bu türden bir eşya tüketir (en sondaki yuvadan başlayarak).</summary>
    public bool TryConsume(ItemType type)
    {
        for (int i = _slots.Length - 1; i >= 0; i--)
        {
            var slot = _slots[i];
            if (slot == null || slot.Type != type) continue;

            slot.Count--;
            if (slot.Count <= 0) _slots[i] = null;
            return true;
        }
        return false;
    }
}
