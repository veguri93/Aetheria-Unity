using System;
using UnityEngine;

// One outstanding exchange across closing/reopening the shop and changing offers.
public static class MerchantPurchaseState
{
    static long sequence, pending;
    static float nextAllowed;
    public static event Action Changed;
    public static bool Pending => pending != 0;
    public static bool CanBuy => !Pending && Time.realtimeSinceStartup >= nextAllowed;
    public static string Status { get; private set; }
    public static void Disconnected(){if(Pending)Complete(pending,false);}
    public static bool Begin(out long id)
    {
        id = 0;
        if (!CanBuy) return false;
        id = pending = ++sequence;
        nextAllowed = Time.realtimeSinceStartup + 0.8f;
        Status = "Purchasing…";
        Changed?.Invoke();
        return true;
    }
    public static void Complete(long id, bool success)
    {
        if (id != pending) return;
        pending = 0;
        Status = success ? "Purchased" : "Purchase failed";
        Changed?.Invoke();
    }
    public static void Handle(string data)
    {
        var parts = data.Split('|');
        if (parts.Length == 2 && long.TryParse(parts[0], out long id)) Complete(id, parts[1] == "1");
    }
}
