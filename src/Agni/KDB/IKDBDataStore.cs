using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.DataAccess;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;


namespace Agni.KDB
{

  public struct KDBRecord<TResult> where TResult : class
  {
    public static readonly KDBRecord<TResult> Unassigned = new KDBRecord<TResult>();

    public KDBRecord(TResult value, int slidingExpirationDays, DateTime lastUseDate, DateTime? absoluteExpirationDateUTC)
    {
      Value = value;
      SlidingExpirationDays = slidingExpirationDays;
      LastUseDate = lastUseDate;
      AbsoluteExpirationDateUTC = absoluteExpirationDateUTC;
    }

    public readonly TResult Value;
    public readonly int SlidingExpirationDays;
    public readonly DateTime LastUseDate;
    public readonly DateTime? AbsoluteExpirationDateUTC;

    public bool IsAssigned { get { return Value != null; } }
  }

  /// <summary>
  /// Stipulates a contract for KDBDataStore
  /// </summary>
  public interface IKDBDataStore : IDataStore
  {
    /// <summary>
    /// Gets a row of data by key, or null if row with such key was not found or data is not Row
    /// </summary>
    /// <param name="table">Table. Required must be non-null valid identifier string less than 32 chars</param>
    /// <param name="key">Byte array key, must be non-null array with at least one element</param>
    Row Get(string table, byte[] key);

    /// <summary>
    ///Gets a row of data projected in the specified typed model, or null if row with such key was not found or data is not Row
    /// </summary>
    /// <param name="table">Table. Required must be non-null valid identifier string less than 32 chars</param>
    /// <param name="key">Byte array key, must be non-null array with at least one element</param>
    /// <param name="dontToch">If true then does not update the item's last use time</param>
    KDBRecord<TRow> Get<TRow>(string table, byte[] key, bool dontToch = false) where TRow : Row;


     /// <summary>
    ///Gets a raw byte[] of data or null if data does not exist or data is not raw byte[] but Row
    /// </summary>
    /// <param name="table">Table. Required must be non-null valid identifier string less than 32 chars</param>
    /// <param name="key">Byte array key, must be non-null array with at least one element</param>
    /// <param name="dontToch">If true then does not update the item's last use time</param>
    KDBRecord<byte[]> GetRaw(string table, byte[] key, bool dontToch = false);


    /// <summary>
    /// Puts a row of data under the specified key
    /// </summary>
    /// <param name="table">Table. Required must be non-null valid identifier string less than 32 chars</param>
    /// <param name="key">Byte array key, must be non-null array with at least one element</param>
    /// <param name="value">Data object must be non-null</param>
    /// <param name="slidingExpirationDays">
    ///  When set, specifies the sliding expiration of the entry in days.
    ///  The system DOES NOT guarantee the instantaneous deletion of expired data
    /// </param>
    /// <param name="absoluteExpirationDateUtc">
    ///  When set, specifies when garbage collector should auto-delete the value.
    ///  It does not guarantee that the value is deleted right at that date
    /// </param>
    void Put(string table, byte[] key, Row value, int slidingExpirationDays = -1, DateTime? absoluteExpirationDateUtc = null);


    /// <summary>
    /// Puts a raw byte[] value under the specified key
    /// </summary>
    /// <param name="table">Table. Required must be non-null valid identifier string less than 32 chars</param>
    /// <param name="key">Byte array key, must be non-null array with at least one element</param>
    /// <param name="value">byte[] must be non-null</param>
    /// <param name="slidingExpirationDays">
    ///  When set, specifies the sliding expiration of the entry in days.
    ///  The system DOES NOT guarantee the instantaneous deletion of expired data
    /// </param>
    /// <param name="absoluteExpirationDateUtc">
    ///  When set, specifies when garbage collector should auto-delete the value.
    ///  It does not guarantee that the value is deleted right at that date
    /// </param>
    void PutRaw(string table, byte[] key, byte[] value, int slidingExpirationDays = -1, DateTime? absoluteExpirationDateUtc = null);

    /// <summary>
    /// Deletes a row of data under the specified key returning true if deletion succeeded
    /// </summary>
    /// <param name="table">Table. Required must be non-null valid identifier string less than 32 chars</param>
    /// <param name="key">Byte array key, must be non-null array with at least one element</param>
    bool Delete(string table, byte[] key);
  }

  public interface IKDBDataStoreImplementation : IKDBDataStore, IDataStoreImplementation
  {

  }

}
