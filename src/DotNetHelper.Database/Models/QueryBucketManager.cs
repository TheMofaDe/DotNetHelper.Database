using System;
using System.Collections.Generic;

namespace DotNetHelper.Database.Models
{
    public class QueryBucketManager : IDisposable
    {
        public int MaxBucketSize { get; set; } = 50;
        public bool IncludeReadableQuery { get; set; } = false;
        internal List<QueryBucket> Bucket { get; private set; } = new List<QueryBucket>() { };
        public delegate void FullQueryBucketEventHandler(object sender, FullQueryBucketEventArgs e);
        public event FullQueryBucketEventHandler FullBucketReached;


        public delegate void OnBeforeAddToBucketEventHandler(object sender, QueryBucketEventArgs e);
        public event OnBeforeAddToBucketEventHandler BeforeAddToBucket;

        internal object ThreadSafe = new object();

        public QueryBucketManager()
        {

        }
        public void AddBucket(QueryBucket bucket)
        {
            lock (ThreadSafe)
            {
                if (Bucket.Count >= MaxBucketSize)
                {
                    Bucket.Add(bucket);
                    OnFullBucket(new FullQueryBucketEventArgs(Bucket) { });
                    Bucket.Clear();
                }
                else
                {
                    OnBeforeAdd(new QueryBucketEventArgs(bucket) { });
                    Bucket.Add(bucket);
                }
            }
        }

        protected virtual void OnFullBucket(FullQueryBucketEventArgs e)
        {
            FullBucketReached?.Invoke(this, e);
        }


        protected virtual void OnBeforeAdd(QueryBucketEventArgs e)
        {
            BeforeAddToBucket?.Invoke(this, e);
        }



        public void Dispose()
        {

            Bucket?.Clear();
            Bucket = null;
            if (FullBucketReached != null)
                foreach (var d in FullBucketReached?.GetInvocationList())
                {
                    FullBucketReached -= (FullQueryBucketEventHandler)d;
                }
            if (BeforeAddToBucket != null)
                foreach (var d in BeforeAddToBucket?.GetInvocationList())
                {
                    BeforeAddToBucket -= (OnBeforeAddToBucketEventHandler)d;
                }
        }
    }

    public class FullQueryBucketEventArgs : EventArgs
    {
        public List<QueryBucket> BucketOfQueries { get; }

        public FullQueryBucketEventArgs(List<QueryBucket> buckets)
        {
            BucketOfQueries = buckets;
        }
    }


    public class QueryBucketEventArgs : EventArgs
    {
        public QueryBucket QueryBucket { get; }

        public QueryBucketEventArgs(QueryBucket bucket)
        {
            QueryBucket = bucket;
        }
    }

}
