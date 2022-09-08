using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using DAL;
using System.Collections.Generic;
using System.Linq;
using Dapper.LiteSql;
using Utils;
using System.Data.Common;
using System.Threading.Tasks;
using System.Threading;

namespace Dapper.LiteSqlTest
{
    /// <summary>
    /// 测试session和事务的跨线程使用
    /// 通常建议一个线程一个session，一个session对应一个数据库连接和事务
    /// 多线程并发的情况建议每个线程中通过LiteSqlClient实例GetSession
    /// session和事务支持跨线程使用是为了特殊情况或者基于LiteSql开发Web框架
    /// </summary>
    [TestClass]
    public class MultiThreadTest
    {
        /// <summary>
        /// 任务数量
        /// </summary>
        private int _count = 1000;

        /// <summary>
        /// 独立线程池
        /// 当任务数量较大，且每个任务都开启一个线程的情况下，如果不限制使用线程数量，线程池被占满后性能非常差乃至报错。
        /// 线程不是越少越好，也不是越多越好。
        /// </summary>
        private TaskSchedulerEx _task = new TaskSchedulerEx(0, 30);

        #region 构造函数
        public MultiThreadTest()
        {
            ServiceHelper.Get<BsOrderDal>().Preheat();
        }
        #endregion

        #region RunTask
        private Task RunTask(Action action)
        {
            return _task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                }
            });
        }

        private Task RunTask<T>(Action<T> action, T t)
        {
            return _task.Run(obj =>
            {
                try
                {
                    action((T)obj);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                }
            }, t);
        }
        #endregion

        #region 多线程并发插入(不带事务)
        [TestMethod]
        public void Test11Insert()
        {
            List<SysUser> userList = new List<SysUser>();
            for (int i = 1; i <= _count; i++)
            {
                SysUser user = new SysUser();
                user.UserName = "testUser";
                user.RealName = "测试插入用户";
                user.Password = "123456";
                user.CreateUserid = "1";
                user.CreateTime = DateTime.Now;
                userList.Add(user);
            }

            var session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

            try
            {
                List<Task> tasks = new List<Task>();
                foreach (SysUser item in userList)
                {
                    var task = RunTask(user =>
                    {
                        session.Insert(user);
                    }, item);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());

                List<SysUser> list = session.Queryable<SysUser>().Where(t => t.Id > 20).ToList();
                Assert.IsTrue(list.Count >= _count);
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region 多线程并发插入(开启事务)
        [TestMethod]
        public void Test21Insert_Tran()
        {
            List<SysUser> userList = new List<SysUser>();
            for (int i = 1; i <= _count; i++)
            {
                SysUser user = new SysUser();
                user.UserName = "testUser";
                user.RealName = "测试插入用户";
                user.Password = "123456";
                user.CreateUserid = "1";
                user.CreateTime = DateTime.Now;
                userList.Add(user);
            }

            var session = LiteSqlFactory.GetSession();
            session.OnExecuting = (s, p) => Console.WriteLine(s); //打印SQL

            try
            {
                session.BeginTransaction();
                List<Task> tasks = new List<Task>();
                foreach (SysUser item in userList)
                {
                    var task = RunTask(user =>
                    {
                        session.Insert(user);
                    }, item);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
                session.CommitTransaction();

                List<SysUser> list = session.Queryable<SysUser>().Where(t => t.Id > 20).ToList();
                Assert.IsTrue(list.Count >= _count);
            }
            catch
            {
                session.RollbackTransaction();
                throw;
            }
        }
        #endregion

        #region 多线程并发更新(不带事务)
        [TestMethod]
        public void Test12Update()
        {
            var session = LiteSqlFactory.GetSession();
            List<SysUser> list = session.Queryable<SysUser>().Where(t => t.Id > 20).ToList();
            Random rnd = new Random();

            try
            {
                session.AttachOld(list);
                foreach (SysUser user in list)
                {
                    user.Remark = "1测试修改用户" + rnd.Next(1, 10000);
                    user.UpdateUserid = "1";
                    user.UpdateTime = DateTime.Now;
                }

                List<Task> tasks = new List<Task>();
                foreach (SysUser item in list)
                {
                    var task = RunTask(user =>
                    {
                        session.Update(user);
                    }, item);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());

                list = session.Queryable<SysUser>().Where(t => t.Id > 20 && t.Remark.Contains("1测试修改用户")).ToList();
                Assert.IsTrue(list.Count >= _count);
            }
            catch
            {
                session.RollbackTransaction();
                throw;
            }
        }
        #endregion

        #region 多线程并发更新(开启事务)
        [TestMethod]
        public void Test12Update_Tran()
        {
            var session = LiteSqlFactory.GetSession();
            List<SysUser> list = session.Queryable<SysUser>().Where(t => t.Id > 20).ToList();
            Random rnd = new Random();

            try
            {
                session.AttachOld(list);
                foreach (SysUser user in list)
                {
                    user.Remark = "2测试修改用户" + rnd.Next(1, 10000);
                    user.UpdateUserid = "1";
                    user.UpdateTime = DateTime.Now;
                }

                session.BeginTransaction();
                List<Task> tasks = new List<Task>();
                foreach (SysUser item in list)
                {
                    var task = RunTask(user =>
                    {
                        session.Update(user);
                    }, item);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
                session.CommitTransaction();

                list = session.Queryable<SysUser>().Where(t => t.Id > 20 && t.Remark.Contains("2测试修改用户")).ToList();
                Assert.IsTrue(list.Count >= _count);
            }
            catch
            {
                session.RollbackTransaction();
                throw;
            }
        }
        #endregion

        #region 多线程并发删除(不带事务)
        [TestMethod]
        public void Test19Delete()
        {
            var session = LiteSqlFactory.GetSession();
            List<SysUser> list = session.Queryable<SysUser>().Where(t => t.Id > 20).ToList();
            Random rnd = new Random();

            try
            {
                List<Task> tasks = new List<Task>();
                foreach (SysUser item in list)
                {
                    var task = RunTask(user =>
                    {
                        session.DeleteById<SysUser>(user.Id);
                    }, item);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
            }
            catch
            {
                session.RollbackTransaction();
                throw;
            }

            long count = session.Queryable<SysUser>().Where(t => t.Id > 20).Count();
            Assert.IsTrue(count == 0);
        }
        #endregion

        #region 多线程并发删除(开启事务)
        [TestMethod]
        public void Test29Delete_Tran()
        {
            var session = LiteSqlFactory.GetSession();
            List<SysUser> list = session.Queryable<SysUser>().Where(t => t.Id > 20).ToList();
            Random rnd = new Random();

            try
            {
                session.BeginTransaction();
                List<Task> tasks = new List<Task>();
                foreach (SysUser item in list)
                {
                    var task = RunTask(user =>
                    {
                        session.DeleteById<SysUser>(user.Id);
                    }, item);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
                session.CommitTransaction();
            }
            catch
            {
                session.RollbackTransaction();
                throw;
            }

            long count = session.Queryable<SysUser>().Where(t => t.Id > 20).Count();
            Assert.IsTrue(count == 0);
        }
        #endregion

        #region 多线程并发查询
        [TestMethod]
        public void Test13Query()
        {
            var session = LiteSqlFactory.GetSession();

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                var task = RunTask(obj =>
                {
                    List<SysUser> list = session.Queryable<SysUser>().Where(t => t.Id <= 20).ToList();
                    Assert.IsTrue(list.Count > 0);
                    if (obj == 0)
                    {
                        foreach (SysUser item in list)
                        {
                            Console.WriteLine(ModelToStringUtil.ToString(item));
                        }
                    }
                }, i);
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());

            List<SysUser> list2 = session.Queryable<SysUser>().Where(t => t.Id > 20).ToList();
            Assert.IsTrue(list2.Count >= _count);
        }
        #endregion

    }
}
