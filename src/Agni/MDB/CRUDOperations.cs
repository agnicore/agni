using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

using NFX;
using NFX.DataAccess;
using NFX.DataAccess.CRUD;

namespace Agni.MDB
{
  /// <summary>
  /// Provides facade for ICrudOperations and ICRUDTransactionOperations
  /// executed against the particular shard returned by the MDB areas partition / routing
  /// </summary>
  public struct CRUDOperations : ICRUDOperations, ICRUDTransactionOperations
  {

    internal CRUDOperations(MDBArea.Partition.Shard shard)
    {
      this.Shard = shard;
    }

    /// <summary>
    /// The shard that services this instance
    /// </summary>
    public readonly MDBArea.Partition.Shard Shard;



    public int Delete(Row row, IDataStoreKey key = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Delete(row, key);
    }

    public Task<int> DeleteAsync(Row row, IDataStoreKey key = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.DeleteAsync(row, key);
    }

    public int ExecuteWithoutFetch(params Query[] queries)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.ExecuteWithoutFetch(queries);
    }

    public Task<int> ExecuteWithoutFetchAsync(params Query[] queries)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.ExecuteWithoutFetchAsync(queries);
    }

    public Schema GetSchema(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.GetSchema(query);
    }

    public Task<Schema> GetSchemaAsync(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.GetSchemaAsync(query);
    }

    public int Insert(Row row, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Insert(row, filter);
    }

    public Task<int> InsertAsync(Row row, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.InsertAsync(row, filter);
    }

    public List<RowsetBase> Load(params Query[] queries)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Load(queries);
    }

    public Task<List<RowsetBase>> LoadAsync(params Query[] queries)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.LoadAsync(queries);
    }

    public Row LoadOneRow(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.LoadOneRow(query);
    }

    public Task<Row> LoadOneRowAsync(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.LoadOneRowAsync(query);
    }

    public RowsetBase LoadOneRowset(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.LoadOneRowset(query);
    }

    public Task<RowsetBase> LoadOneRowsetAsync(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.LoadOneRowsetAsync(query);
    }

    public Cursor OpenCursor(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.OpenCursor(query);
    }

    public Task<Cursor> OpenCursorAsync(Query query)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.OpenCursorAsync(query);
    }

    public int Save(params RowsetBase[] rowsets)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Save(rowsets);
    }

    public Task<int> SaveAsync(params RowsetBase[] rowsets)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.SaveAsync(rowsets);
    }

    public bool SupportsTrueAsynchrony
    {
      get { return Shard.Area.PhysicalDataStore.SupportsTrueAsynchrony; }
    }

    public int Update(Row row, IDataStoreKey key = null, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Update(row, key, filter);
    }

    public Task<int> UpdateAsync(Row row, IDataStoreKey key = null, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.UpdateAsync(row, key, filter);
    }

    public int Upsert(Row row, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.Upsert(row, filter);
    }

    public Task<int> UpsertAsync(Row row, FieldFilterFunc filter = null)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.UpsertAsync(row, filter);
    }

    public CRUDTransaction BeginTransaction(IsolationLevel iso = IsolationLevel.ReadCommitted, TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.BeginTransaction(iso, behavior);
    }

    public Task<CRUDTransaction> BeginTransactionAsync(IsolationLevel iso = IsolationLevel.ReadCommitted, TransactionDisposeBehavior behavior = TransactionDisposeBehavior.CommitOnDispose)
    {
      using(new CRUDOperationCallContext{ConnectString = Shard.EffectiveConnectionString})
        return Shard.Area.PhysicalDataStore.BeginTransactionAsync(iso, behavior);
    }

    public bool SupportsTransactions
    {
      get { return Shard.Area.PhysicalDataStore.SupportsTransactions; }
    }
  }
}
