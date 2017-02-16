using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using Dapper;

namespace PayAtTable.API.Helpers
{

//    public class TwoIntKey
//    {
//        public int First { get; set; }
//        public int Second { get; set; }
        
//public override bool Equals(object obj)
//        {
//            var commands = (TwoIntKey)obj;
//            return (commands.First == First) && (commands.Second == Second);
//        }
//    }

    public static class DbHelpers
    {


        #region MultiMap helpers
        public static IEnumerable<TParentType> MultiMap<TParentType, TChildType, TKeyType>
            (
            IEnumerable<TParentType> parents,
            IEnumerable<TChildType> children,
            Func<TParentType, TKeyType> firstKey,
            Func<TChildType, TKeyType> secondKey,
            Action<TParentType, IEnumerable<TChildType>> addChildren
            )
        {
            //var parentsList = parents.ToList();
            var childMap = children.GroupBy(s => secondKey(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());

            foreach (var item in parents)
            {
                IEnumerable<TChildType> childrenFiltered;
                if (childMap.TryGetValue(firstKey(item), out childrenFiltered))
                {
                    addChildren(item, childrenFiltered);
                }
            }

            return parents;
        }


        public static IEnumerable<TParentType> MultiMap<TParentType, TChild1Type, TChild2Type, TKeyType>
            (
            IEnumerable<TParentType> parents,
            IEnumerable<TChild1Type> children1,
            IEnumerable<TChild2Type> children2,
            Func<TParentType, TKeyType> parentKey,
            Func<TChild1Type, TKeyType> child1Key,
            Func<TChild2Type, TKeyType> child2Key,
            Action<TParentType, IEnumerable<TChild1Type>, IEnumerable<TChild2Type>> addChildren
            )
        {
            var child1Map = children1.GroupBy(s => child1Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child2Map = children2.GroupBy(s => child2Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());


            foreach (var item in parents)
            {
                IEnumerable<TChild1Type> children1Filtered = null;
                IEnumerable<TChild2Type> children2Filtered = null;

                if (child1Map.TryGetValue(parentKey(item), out children1Filtered) || child2Map.TryGetValue(parentKey(item), out children2Filtered))
                {
                    addChildren(item, children1Filtered, children2Filtered);
                }
            }

            return parents;
        }


        public static IEnumerable<TParentType> MultiMap<TParentType, TChild1Type, TChild2Type, TChild3Type, TKeyType>
            (
            IEnumerable<TParentType> parents,
            IEnumerable<TChild1Type> children1,
            IEnumerable<TChild2Type> children2,
            IEnumerable<TChild3Type> children3,
            Func<TParentType, TKeyType> parentKey,
            Func<TChild1Type, TKeyType> child1Key,
            Func<TChild2Type, TKeyType> child2Key,
            Func<TChild3Type, TKeyType> child3Key,
            Action<TParentType, IEnumerable<TChild1Type>> addChildren1,
            Action<TParentType, IEnumerable<TChild2Type>> addChildren2,
            Action<TParentType, IEnumerable<TChild3Type>> addChildren3
            )
        {
            var child1Map = children1.GroupBy(s => child1Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child2Map = children2.GroupBy(s => child2Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child3Map = children3.GroupBy(s => child3Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());


            foreach (var item in parents)
            {
                IEnumerable<TChild1Type> children1Filtered;
                if (child1Map.TryGetValue(parentKey(item), out children1Filtered))
                {
                    addChildren1(item, children1Filtered);
                }

                IEnumerable<TChild2Type> children2Filtered;
                if (child2Map.TryGetValue(parentKey(item), out children2Filtered))
                {
                    addChildren2(item, children2Filtered);
                }
                
                IEnumerable<TChild3Type> children3Filtered;
                if (child3Map.TryGetValue(parentKey(item), out children3Filtered))
                {
                    addChildren3(item, children3Filtered);
                }
            }

            return parents;
        }


        public static IEnumerable<TParentType> MultiMap<TParentType, TChild1Type, TChild2Type, TChild3Type, TChild4Type, TKeyType>
            (
            IEnumerable<TParentType> parents,
            IEnumerable<TChild1Type> children1,
            IEnumerable<TChild2Type> children2,
            IEnumerable<TChild3Type> children3,
            IEnumerable<TChild4Type> children4,
            Func<TParentType, TKeyType> parentKey,
            Func<TChild1Type, TKeyType> child1Key,
            Func<TChild2Type, TKeyType> child2Key,
            Func<TChild3Type, TKeyType> child3Key,
            Func<TChild4Type, TKeyType> child4Key,
            Action<TParentType, IEnumerable<TChild1Type>> addChildren1,
            Action<TParentType, IEnumerable<TChild2Type>> addChildren2,
            Action<TParentType, IEnumerable<TChild3Type>> addChildren3,
            Action<TParentType, IEnumerable<TChild4Type>> addChildren4
            )
        {
            var child1Map = children1.GroupBy(s => child1Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child2Map = children2.GroupBy(s => child2Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child3Map = children3.GroupBy(s => child3Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child4Map = children4.GroupBy(s => child4Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());


            foreach (var item in parents)
            {
                IEnumerable<TChild1Type> children1Filtered;
                if (child1Map.TryGetValue(parentKey(item), out children1Filtered))
                {
                    addChildren1(item, children1Filtered);
                }

                IEnumerable<TChild2Type> children2Filtered;
                if (child2Map.TryGetValue(parentKey(item), out children2Filtered))
                {
                    addChildren2(item, children2Filtered);
                }

                IEnumerable<TChild3Type> children3Filtered;
                if (child3Map.TryGetValue(parentKey(item), out children3Filtered))
                {
                    addChildren3(item, children3Filtered);
                }

                IEnumerable<TChild4Type> children4Filtered;
                if (child4Map.TryGetValue(parentKey(item), out children4Filtered))
                {
                    addChildren4(item, children4Filtered);
                }
            }

            return parents;
        }


        public static IEnumerable<TParentType> MultiMap<TParentType, TChild1Type, TChild2Type, TChild3Type, TChild4Type, TChild5Type, TKeyType>
            (
            IEnumerable<TParentType> parents,
            IEnumerable<TChild1Type> children1,
            IEnumerable<TChild2Type> children2,
            IEnumerable<TChild3Type> children3,
            IEnumerable<TChild4Type> children4,
            IEnumerable<TChild5Type> children5,
            Func<TParentType, TKeyType> parentKey,
            Func<TChild1Type, TKeyType> child1Key,
            Func<TChild2Type, TKeyType> child2Key,
            Func<TChild3Type, TKeyType> child3Key,
            Func<TChild4Type, TKeyType> child4Key,
            Func<TChild5Type, TKeyType> child5Key,
            Action<TParentType, IEnumerable<TChild1Type>> addChildren1,
            Action<TParentType, IEnumerable<TChild2Type>> addChildren2,
            Action<TParentType, IEnumerable<TChild3Type>> addChildren3,
            Action<TParentType, IEnumerable<TChild4Type>> addChildren4,
            Action<TParentType, IEnumerable<TChild5Type>> addChildren5
            )
        {
            var child1Map = children1.GroupBy(s => child1Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child2Map = children2.GroupBy(s => child2Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child3Map = children3.GroupBy(s => child3Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child4Map = children4.GroupBy(s => child4Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child5Map = children5.GroupBy(s => child5Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());


            foreach (var item in parents)
            {
                IEnumerable<TChild1Type> children1Filtered;
                if (child1Map.TryGetValue(parentKey(item), out children1Filtered))
                {
                    addChildren1(item, children1Filtered);
                }

                IEnumerable<TChild2Type> children2Filtered;
                if (child2Map.TryGetValue(parentKey(item), out children2Filtered))
                {
                    addChildren2(item, children2Filtered);
                }

                IEnumerable<TChild3Type> children3Filtered;
                if (child3Map.TryGetValue(parentKey(item), out children3Filtered))
                {
                    addChildren3(item, children3Filtered);
                }

                IEnumerable<TChild4Type> children4Filtered;
                if (child4Map.TryGetValue(parentKey(item), out children4Filtered))
                {
                    addChildren4(item, children4Filtered);
                }

                IEnumerable<TChild5Type> children5Filtered;
                if (child5Map.TryGetValue(parentKey(item), out children5Filtered))
                {
                    addChildren5(item, children5Filtered);
                }
            }

            return parents;
        }



        public static IEnumerable<TParentType> MultiMap<TParentType, TChild1Type, TChild2Type, TChild3Type, TChild4Type, TChild5Type, TChild6Type, TKeyType>
            (
            IEnumerable<TParentType> parents,
            IEnumerable<TChild1Type> children1,
            IEnumerable<TChild2Type> children2,
            IEnumerable<TChild3Type> children3,
            IEnumerable<TChild4Type> children4,
            IEnumerable<TChild5Type> children5,
            IEnumerable<TChild6Type> children6,
            Func<TParentType, TKeyType> parentKey,
            Func<TChild1Type, TKeyType> child1Key,
            Func<TChild2Type, TKeyType> child2Key,
            Func<TChild3Type, TKeyType> child3Key,
            Func<TChild4Type, TKeyType> child4Key,
            Func<TChild5Type, TKeyType> child5Key,
            Func<TChild6Type, TKeyType> child6Key,
            Action<TParentType, IEnumerable<TChild1Type>> addChildren1,
            Action<TParentType, IEnumerable<TChild2Type>> addChildren2,
            Action<TParentType, IEnumerable<TChild3Type>> addChildren3,
            Action<TParentType, IEnumerable<TChild4Type>> addChildren4,
            Action<TParentType, IEnumerable<TChild5Type>> addChildren5,
            Action<TParentType, IEnumerable<TChild6Type>> addChildren6
            )
        {
            var child1Map = children1.GroupBy(s => child1Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child2Map = children2.GroupBy(s => child2Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child3Map = children3.GroupBy(s => child3Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child4Map = children4.GroupBy(s => child4Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child5Map = children5.GroupBy(s => child5Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
            var child6Map = children6.GroupBy(s => child6Key(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());


            foreach (var item in parents)
            {
                IEnumerable<TChild1Type> children1Filtered;
                if (child1Map.TryGetValue(parentKey(item), out children1Filtered))
                {
                    addChildren1(item, children1Filtered);
                }

                IEnumerable<TChild2Type> children2Filtered;
                if (child2Map.TryGetValue(parentKey(item), out children2Filtered))
                {
                    addChildren2(item, children2Filtered);
                }

                IEnumerable<TChild3Type> children3Filtered;
                if (child3Map.TryGetValue(parentKey(item), out children3Filtered))
                {
                    addChildren3(item, children3Filtered);
                }

                IEnumerable<TChild4Type> children4Filtered;
                if (child4Map.TryGetValue(parentKey(item), out children4Filtered))
                {
                    addChildren4(item, children4Filtered);
                }

                IEnumerable<TChild5Type> children5Filtered;
                if (child5Map.TryGetValue(parentKey(item), out children5Filtered))
                {
                    addChildren5(item, children5Filtered);
                }

                IEnumerable<TChild6Type> children6Filtered;
                if (child6Map.TryGetValue(parentKey(item), out children6Filtered))
                {
                    addChildren6(item, children6Filtered);
                }
            }

            return parents;
        }
        #endregion



        //public static IEnumerable<TParentType> MultiMap<TParentType, TChildType, TSubChildType, TKeyType, TSubKeyType>
        //    (
        //    IEnumerable<TParentType> parents,
        //    IEnumerable<TChildType> children,
        //    IEnumerable<TSubChildType> subChildren,
        //    Func<TParentType, TKeyType> firstKey,
        //    Func<TChildType, TKeyType> secondKey,
        //    Func<TSubChildType, TKeyType> thirdKey,
        //    Action<TParentType, IEnumerable<TChildType>> addChildren,
        //    Action<TChildType, IEnumerable<TSubChildType>> addSubChildren
        //    )
        //{
        //    //var parentsList = parents.ToList();
        //    var childMap = children.GroupBy(s => secondKey(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());
        //    var subChildMap = subChildren.GroupBy(s => thirdKey(s)).ToDictionary(g => g.Key, g => g.AsEnumerable());

        //    foreach (var item in parents)
        //    {
        //        IEnumerable<TChildType> childrenFiltered;
        //        if (childMap.TryGetValue(firstKey(item), out childrenFiltered))
        //        {
        //            foreach (var subItem in childrenFiltered)
        //            {
        //                IEnumerable<TSubChildType> subChildrenFiltered;
        //                if (subChildMap.TryGetValue(secondKey(subItem), out subChildrenFiltered))
        //                {
        //                    addSubChildren(subItem, subChildrenFiltered);
        //                }
        //            }   

        //            addChildren(item, childrenFiltered);
        //        }
        //    }

        //    return parents;
        //}


        public static string BuildInsertQuery(string tableName, dynamic data, bool includeIdentity = true, string[] excludeProperties = null)
        {
            var sbNames = new StringBuilder();
            var sbValues = new StringBuilder();

            Type t = data.GetType();
            var properties = t.GetProperties();
            for (int i = 0; i < properties.Count(); i++)
            {
                var property = properties.ElementAt(i);

                // Check for exclusiuons
                if (excludeProperties != null && excludeProperties.Contains(property.Name))
                {
                    continue;
                }

                sbNames.AppendFormat("[{0}]", property.Name);
                sbValues.AppendFormat("@{0}", property.Name);

                if (i < properties.Count() - 1)
                {
                    sbNames.Append(", ");
                    sbValues.AppendFormat(", ");
                }
            }

            var identity = "";
            if (includeIdentity)
                identity = "\r\n SELECT CAST(scope_identity() as int)";

            return String.Format("INSERT INTO [{0}] ({1}) VALUES ({2}){3}", tableName, sbNames.ToString(), sbValues.ToString(), identity);
        }

        public static string BuildDeleteQuery(string tableName, int key)
        {
            return string.Format("DELETE FROM [{0}] WHERE [Id] = {1}", tableName, key);
        }

        public static string BuildDeprecateQuery(string tableName, int key)
        {
            return string.Format("UPDATE [{0}] SET [Deprecated]=1 WHERE [Id] = {1}", tableName, key);
        }

        public static string BuildUpdateQuery(string tableName, int key, dynamic values)
        {
            var sbParams = new StringBuilder();

            Type t = values.GetType();
            var properties = t.GetProperties();
            for (int i = 0; i < properties.Count(); i++)
            {
                var property = properties.ElementAt(i);
                sbParams.AppendFormat("[{0}] = @{0}", property.Name);

                if (i < properties.Count() - 1)
                    sbParams.AppendFormat(", ");
            }


            return String.Format("UPDATE [{0}] SET {1} WHERE [Id] = {2}", tableName, sbParams.ToString(), key);
        }

        public static int GetNVarcharFieldLength(this SqlConnection db, string tableName, string columnName)
        {
            if (db.State != System.Data.ConnectionState.Broken && db.State != System.Data.ConnectionState.Closed)
            {
                var sql = "select (case when max_length = -1 then 4000 else max_length/2 end) [Length] from sys.columns where object_id = object_id(@tableName) and name = @columnName";

                return db.ExecuteScalar<int>(sql, new { tableName = tableName, columnName = columnName });
            }
            return 0;
        }

        public static string GetGlobalSetting(this SqlConnection db, string key)
        {
            if (db.State != System.Data.ConnectionState.Broken && db.State != System.Data.ConnectionState.Closed)
            {
                return db.Query<string>("SELECT top 1 value from GlobalSetting where [key] = @key", new { key = key }).FirstOrDefault();
            }
            return null;
        }

    }
}