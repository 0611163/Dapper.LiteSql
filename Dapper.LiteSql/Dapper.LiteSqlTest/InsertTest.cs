﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using DAL;
using System.Collections.Generic;
using Dapper.LiteSql;
using Utils;
using System.Threading.Tasks;

namespace Dapper.LiteSqlTest
{
    [TestClass]
    public class InsertTest
    {
        #region 变量
        private BsOrderDal m_BsOrderDal = ServiceHelper.Get<BsOrderDal>();
        private SysUserDal m_SysUserDal = ServiceHelper.Get<SysUserDal>();
        #endregion

        #region 构造函数
        public InsertTest()
        {
            m_BsOrderDal.Preheat();
        }
        #endregion

        #region 测试添加订单
        [TestMethod]
        public void TestInsertOrder()
        {
            string userId = "10";

            BsOrder order = new BsOrder();
            order.OrderTime = DateTime.Now;
            order.Amount = 0;
            order.OrderUserid = Convert.ToInt64(userId);
            order.Status = 0;
            order.CreateUserid = userId;

            List<BsOrderDetail> detailList = new List<BsOrderDetail>();
            BsOrderDetail detail = new BsOrderDetail();
            detail.GoodsName = "电脑";
            detail.Quantity = 3;
            detail.Price = 5100;
            detail.Spec = "台";
            detail.CreateUserid = userId;
            detail.OrderNum = 1;
            detailList.Add(detail);

            detail = new BsOrderDetail();
            detail.GoodsName = "鼠标";
            detail.Quantity = 12;
            detail.Price = (decimal)50.68;
            detail.Spec = "个";
            detail.CreateUserid = userId;
            detail.OrderNum = 2;
            detailList.Add(detail);

            detail = new BsOrderDetail();
            detail.GoodsName = "键盘";
            detail.Quantity = 11;
            detail.Price = (decimal)123.66;
            detail.Spec = "个";
            detail.CreateUserid = userId;
            detail.OrderNum = 3;
            detailList.Add(detail);

            string id = m_BsOrderDal.Insert(order, detailList);

            var session = LiteSqlFactory.GetSession();

            bool bl = session.CreateSql("select * from bs_order where id=@Id", new { Id = id }).Exists();
            Assert.IsTrue(bl);

            long count = session.CreateSql("select * from bs_order_detail where order_id=@OrderId", new { OrderId = id }).QueryCount();
            Assert.IsTrue(count == 3);
        }
        #endregion

        #region 测试添加两个订单
        /// <summary>
        /// 测试新增两条订单记录
        /// </summary>
        [TestMethod]
        public void TestInsertTwoOrder()
        {
            TestInsertOrder();
            TestInsertOrder();
        }
        #endregion

        #region 测试添加用户
        [TestMethod]
        public void TestInsertUser()
        {
            SysUser user = new SysUser();
            user.UserName = "testUser";
            user.RealName = "测试插入用户";
            user.Password = "123456";
            user.CreateUserid = "1";

            user.Id = m_SysUserDal.Insert(user);
            Console.WriteLine("user.Id=" + user.Id);
            Assert.IsTrue(user.Id > 0);

            var session = LiteSqlFactory.GetSession();

            bool bl = session.CreateSql("select * from sys_user where id=@Id", new { Id = user.Id }).Exists();
            Assert.IsTrue(bl);
        }
        #endregion

        #region 测试添加用户(异步)
        [TestMethod]
        public async Task TestInsertUserAsync()
        {
            SysUser user = new SysUser();
            user.UserName = "testUser";
            user.RealName = "测试插入用户";
            user.Password = "123456";
            user.CreateUserid = "1";

            long id = await m_SysUserDal.InsertAsync(user);
            Console.WriteLine("user.Id=" + id);
            Assert.IsTrue(id > 0);
            var session = LiteSqlFactory.GetSession();

            bool bl = session.CreateSql("select * from sys_user where id=@Id", new { Id = id }).Exists();
            Assert.IsTrue(bl);
        }
        #endregion

        #region 测试添加订单(事务)
        [TestMethod]
        public void TestInsertOrderTransaction()
        {
            string userId = "10";

            BsOrder order = new BsOrder();
            order.Id = Guid.NewGuid().ToString("N");
            order.OrderTime = DateTime.Now;
            order.Amount = 0;
            order.OrderUserid = Convert.ToInt64(userId);
            order.Status = 0;
            order.CreateUserid = userId;

            BsOrderDetail detail = new BsOrderDetail();
            detail.Id = Guid.NewGuid().ToString("N");
            detail.GoodsName = "电脑";
            detail.Quantity = 3;
            detail.Price = 5100;
            detail.Spec = "台";
            detail.CreateUserid = userId;
            detail.OrderNum = 1;

            var session = LiteSqlFactory.GetSession();

            session.OnExecuting = (s, p) =>
            {
                Console.WriteLine(s); //打印SQL
            };

            try
            {
                session.BeginTransaction();

                decimal amount = 0;

                detail.OrderId = order.Id;
                detail.CreateTime = DateTime.Now;
                amount += detail.Price * detail.Quantity;
                session.Insert(detail);

                int a = int.Parse("abc"); //制造异常

                order.CreateTime = DateTime.Now;
                order.Amount = amount;
                session.Insert(order);

                session.CommitTransaction();
            }
            catch (Exception ex)
            {
                session.RollbackTransaction();
                Console.WriteLine("发生异常回滚事务");
            }

            session = LiteSqlFactory.GetSession();

            bool bl = session.CreateSql("select * from bs_order where id=@Id", new { Id = order.Id }).Exists();
            Assert.IsFalse(bl);

            long count = session.CreateSql("select * from bs_order_detail where order_id=@OrderId", new { OrderId = order.Id }).QueryCount();
            Assert.IsTrue(count == 0);
        }
        #endregion

    }
}
