// Copyright © 2017, 2021, Oracle and/or its affiliates.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License, version 2.0, as
// published by the Free Software Foundation.
//
// This program is also distributed with certain software (including
// but not limited to OpenSSL) that is licensed under separate terms,
// as designated in a particular file or component or in included license
// documentation.  The authors of MySQL hereby grant you an
// additional permission to link the program and your derivative works
// with the separately licensed software that they have included with
// MySQL.
//
// Without limiting anything contained in the foregoing, this file,
// which is part of MySQL Connector/NET, is also subject to the
// Universal FOSS Exception, version 1.0, a copy of which can be found at
// http://oss.oracle.com/licenses/universal-foss-exception.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License, version 2.0, for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using MySqlX.XDevAPI;
using System;
using NUnit.Framework;
using MySql.Data.Failover;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using MySql.Data;
using MySql.Data.Common;

namespace MySqlX.Data.Tests
{
  public class ClientSideFailoverTests : BaseTest
  {
    private string localServerIpv4;
    private string localServerIpv6;

    [OneTimeSetUp]
    public void LocalSetUp()
    {
      //get the local MySql server Ip address, like 127.0.0.1 or ::1
      localServerIpv4 = GetMySqlServerIp();
      localServerIpv6 = GetMySqlServerIp(true);
    }

    [Test]
    public void RandomMethodWithBasicFormatConnectionString()
    {
      int connectionTimeout = 1000;

      // Single host.
      using (var session = MySQLX.GetSession(ConnectionString))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
      }

      // Multiple hosts.
      using (var session = MySQLX.GetSession("server=10.10.10.10, localhost, 20.20.20.20, 30.30.30.30;port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
      }

      // Multiple hosts with IPv6
      using (var session = MySQLX.GetSession("server=10.10.10.10, ::1, 20.20.20.20, 30.30.30.30;port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
      }

      // Multiple hosts using synonyms for "server" connection option.
      using (var session = MySQLX.GetSession("host=10.10.10.10, localhost;port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
      }

      // Multiple hosts. All attempts fail.
      Exception ex = Assert.Throws<MySqlException>(() => MySQLX.GetSession("server= 10.10.10.10, 20.20.20.20 ;port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout));
      Assert.AreEqual("Unable to connect to any of the specified MySQL hosts.", ex.Message);

      // Providing port number as part of the host name.
      ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession("server= 10.10.10.10:33050, 20.20.20.20:33060, localhost:33060 ;port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout));
      Assert.AreEqual("Providing a port number as part of the host address isn't supported when using connection strings in basic format or anonymous objects. Use URI format instead.", ex.Message);
    }

    [Test]
    public void RandomMethodWithUriFormatConnectionString()
    {
      int connectionTimeout = 1000;

      // Single host.
      using (var session = MySQLX.GetSession($"mysqlx://test:test@{Host}:{XPort}?connecttimeout={connectionTimeout}"))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
      }

      // Single host and port as an array. Successful connection.
      using (var session = MySQLX.GetSession($"mysqlx://test:test@[{localServerIpv4}:" + XPort + "]?connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
      }

      // Single host as an array. Failed connection.
      Exception ex = Assert.Throws<TimeoutException>(() => MySQLX.GetSession("mysqlx://test:test@[192.1.10.10:" + XPort + "]?connecttimeout=" + connectionTimeout));

      // Multiple hosts.
      using (var session = MySQLX.GetSession($"mysqlx://test:test@[192.1.10.10,{localServerIpv4}:" + XPort + "]?connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
      }

      // Multiple hosts and a schema.
      using (var session = MySQLX.GetSession($"mysqlx://test:test@[192.1.10.10,{localServerIpv4}:" + XPort + "]/test?connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
      }

      // Multiple hosts which may or may not contain a port number.
      using (var session = MySQLX.GetSession("mysqlx://test:test@[192.1.10.10,120.0.0.2:22000,[::1]:" + XPort + "]/test?connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
      }
    }

    [Test]
    public void RandomMethodWithAnonymousTypes()
    {
      int connectionTimeout = 1000;
      string uid = "test";
      string password = "test";

      // Single host.
      using (var session = MySQLX.GetSession(new { server = "localhost", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
      }

      // Multiple hosts.
      using (var session = MySQLX.GetSession(new { server = "10.10.10.10, localhost", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
      }

      // Multiple hosts with IPv6
      using (var session = MySQLX.GetSession(new { server = "10.10.10.10, ::1", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
      }

      // Multiple hosts using synonyms for "server" connection option. First attempt fails, second is succesful.
      using (var session = MySQLX.GetSession(new { datasource = "10.10.10.10, localhost", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
      }

      // Multiple hosts. All attempts fail.
      Exception ex = Assert.Throws<MySqlException>(() => MySQLX.GetSession(new { server = "10.10.10.10, 20.20.20.20", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }));
      Assert.AreEqual("Unable to connect to any of the specified MySQL hosts.", ex.Message);

      // Providing port number as part of the host name.
      ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession(new { server = "10.10.10.10:33060, 20.20.20.20:33060", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }));
      Assert.AreEqual("Providing a port number as part of the host address isn't supported when using connection strings in basic format or anonymous objects. Use URI format instead.", ex.Message);
    }

    [Test]
    public void PriorityMethodWithBasicFormatConnectionString()
    {
      int connectionTimeout = 1000;

      // Single host with max_connections.
      try
      {
        List<Session> sessions = new List<Session>();
        ExecuteSqlAsRoot("SET @@global.mysqlx_max_connections = 2");
        for (int i = 0; i <= 2; i++)
        {
          Session newSession = MySQLX.GetSession($"server=(address={localServerIpv4},priority=100);port=" + XPort + ";uid=test;password=test;");
          sessions.Add(newSession);
        }
        Assert.False(true, "MySqlException should be thrown");
      }
      catch (MySqlException exception)
      {
        Assert.AreEqual(ResourcesX.UnableToOpenSession, exception.Message);
      }
      finally
      {
        ExecuteSqlAsRoot("SET @@global.mysqlx_max_connections = 100");
      }

      using (var session = MySQLX.GetSession($"server=(address={localServerIpv4},priority=100);port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
        Assert.AreEqual(localServerIpv4, session.Settings.Server);
      }

      using (var session = MySQLX.GetSession($"server=(address=server.example,priority=50),(address={localServerIpv4},priority=100);port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
        Assert.AreEqual(localServerIpv4, session.Settings.Server);
      }

      using (var session = MySQLX.GetSession($"server=(address=server.example,priority=100),(address={localServerIpv4},priority=25),(address=192.0.10.56,priority=75);port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
        Assert.AreEqual(localServerIpv4, session.Settings.Server);
      }

      using (var session = MySQLX.GetSession(new
      {
        server = $"(address = {localServerIpv4}, priority = 0),(address=192.0.10.56, priority=100)",
        port = XPort,
        user = "test",
        password = "test",
        sslmode = MySqlSslMode.None
      }))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
        Assert.AreEqual(localServerIpv4, session.Settings.Server);
      }

      using (var session = MySQLX.GetSession($"server=(address=server.example,priority=100),(address={localServerIpv4},priority=25),(address=192.0.10.56,priority=75);port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
        Assert.AreEqual(localServerIpv4, session.Settings.Server);
        Assert.AreEqual("server.example", FailoverManager.FailoverGroup.Hosts[0].Host);
        Assert.AreEqual("192.0.10.56", FailoverManager.FailoverGroup.Hosts[1].Host);
        Assert.AreEqual(localServerIpv4, FailoverManager.FailoverGroup.Hosts[2].Host);
      }

      // Priority outside the 0-100 allowed range.
      Exception ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession($"server=(address=server.example,priority=-20),(address={localServerIpv4},priority=100);port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout));
      Assert.AreEqual("The priority must be between 0 and 100.", ex.Message);

      ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession($"server=(address=server.example,priority=-50),(address={localServerIpv4},priority=101);port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout));
      Assert.AreEqual("The priority must be between 0 and 100.", ex.Message);

      // Set priority for a subset of the hosts.
      ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession($"server=(address=server.example),(address={localServerIpv4},priority=100);port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout));
      Assert.AreEqual("You must either assign no priority to any of the hosts or give a priority for every host.", ex.Message);
      ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession($"server=(address=server.example,priority=50),(address={localServerIpv4});port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout));
      Assert.AreEqual("You must either assign no priority to any of the hosts or give a priority for every host.", ex.Message);
      ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession($"server=(address=server.example,priority=50),(address={localServerIpv4},priority=100),(address=server.example);port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout));
      Assert.AreEqual("You must either assign no priority to any of the hosts or give a priority for every host.", ex.Message);

      // Automatically set priority if no priority is given.
      string hostList = string.Empty;
      int priority = 100;
      for (int i = 1; i <= 105; i++)
      {
        hostList += "(address=server" + i + ".example,priority=" + (priority != 0 ? priority-- : 0) + "),";
        if (i == 105) hostList += "(address=localhost,priority=0)";
      }

      using (var session = MySQLX.GetSession("server=" + hostList + ";port=" + XPort + ";uid=test;password=test;connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
        priority = 100;
        foreach (var host in FailoverManager.FailoverGroup.Hosts)
        {
          Assert.AreEqual(priority != 0 ? priority-- : 0, host.Priority);
        }
      }
    }

    [Test]
    public void PriorityMethodWithUriFormatConnectonString()
    {
      int connectionTimeout = 1000;

      // Single host with max_connections.
      try
      {
        List<Session> sessions = new List<Session>();
        ExecuteSqlAsRoot("SET @@global.mysqlx_max_connections = 2");
        for (int i = 0; i <= 2; i++)
        {
          Session newSession = MySQLX.GetSession($"mysqlx://test:test@[(address={Host}:" + XPort + ",priority=50)]?connecttimeout=" + connectionTimeout);
          sessions.Add(newSession);
        }
        Assert.False(true, "MySqlException should be thrown");
      }
      catch (MySqlException exception)
      {
        Assert.AreEqual(ResourcesX.UnableToOpenSession, exception.Message);
      }
      finally
      {
        ExecuteSqlAsRoot("SET @@global.mysqlx_max_connections = 100");
      }

      using (var session = MySQLX.GetSession($"mysqlx://test:test@[(address=server.example,priority=50),(address={localServerIpv4}:{XPort},priority=100)]?connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
        Assert.AreEqual(localServerIpv4, session.Settings.Server);
      }

      using (var session = MySQLX.GetSession($"mysqlx://test:test@[(address=server.example,priority=50),(address={localServerIpv4}:{XPort},priority=100)]?connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
        Assert.AreEqual(localServerIpv4, session.Settings.Server);
      }

      using (var session = MySQLX.GetSession($"mysqlx://test:test@[(address=server.example,priority=100),(address={localServerIpv4}:{XPort},priority=25),(address=192.0.10.56,priority=75)]?connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
        Assert.AreEqual(localServerIpv4, session.Settings.Server);
        Assert.AreEqual("server.example", FailoverManager.FailoverGroup.Hosts[0].Host);
        Assert.AreEqual("192.0.10.56", FailoverManager.FailoverGroup.Hosts[1].Host);
        Assert.AreEqual(localServerIpv4, FailoverManager.FailoverGroup.Hosts[2].Host);
      }

      // Priority outside the 0-100 allowed range.
      Exception ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession($"mysqlx://test:test@[(address=server.example,priority=-20),(address={localServerIpv4}:{XPort},priority=100)]?connecttimeout=" + connectionTimeout));
      Assert.AreEqual("The priority must be between 0 and 100.", ex.Message);

      ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession($"mysqlx://test:test@[(address=server.example,priority=50),(address={localServerIpv4}:{XPort},priority=101)]?connecttimeout=" + connectionTimeout));
      Assert.AreEqual("The priority must be between 0 and 100.", ex.Message);

      // Set priority for a subset of the hosts.
      ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession($"mysqlx://test:test@[(address=server.example),(address={localServerIpv4}:{XPort},priority=100)]?connecttimeout=" + connectionTimeout));
      Assert.AreEqual("You must either assign no priority to any of the hosts or give a priority for every host.", ex.Message);
      ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession($"mysqlx://test:test@[(address=server.example,priority=100),(address={localServerIpv4}:{XPort})]?connecttimeout=" + connectionTimeout));
      Assert.AreEqual("You must either assign no priority to any of the hosts or give a priority for every host.", ex.Message);
      ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession($"mysqlx://test:test@[(address=server.example),(address={localServerIpv4}:{XPort}),(address=server2.example,priority=100)]?connecttimeout=" + connectionTimeout));
      Assert.AreEqual("You must either assign no priority to any of the hosts or give a priority for every host.", ex.Message);

      // Automatically set priority if no priority is given.
      string hostList = string.Empty;
      int priority = 100;
      for (int i = 1; i <= 105; i++)
      {
        hostList += "(address=server" + i + ".example,priority=" + (priority != 0 ? priority-- : 0) + "),";
        if (i == 105) hostList += $"(address=localhost:{XPort},priority=0)";
      }

      using (var session = MySQLX.GetSession($"mysqlx://test:test@[{hostList}]?connecttimeout=" + connectionTimeout))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
        priority = 100;
        foreach (var host in FailoverManager.FailoverGroup.Hosts)
        {
          Assert.AreEqual(priority != 0 ? priority-- : 0, host.Priority);
        }
      }
    }

    [Test]
    public void PriorityMethodWithAnonymousTypes()
    {
      int connectionTimeout = 1000;
      string uid = "test";
      string password = "test";

      // Single host with max_connections.
      try
      {
        List<Session> sessions = new List<Session>();
        ExecuteSqlAsRoot("SET @@global.mysqlx_max_connections = 2");
        for (int i = 0; i <= 2; i++)
        {
          Session newSession = MySQLX.GetSession(new { server = $"(address={localServerIpv4},priority=100)", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout });
          sessions.Add(newSession);
        }
        Assert.False(true, "MySqlException should be thrown");
      }
      catch (MySqlException exception)
      {
        Assert.AreEqual(ResourcesX.UnableToOpenSession, exception.Message);
      }
      finally
      {
        ExecuteSqlAsRoot("SET @@global.mysqlx_max_connections = 100");
      }

      using (var session = MySQLX.GetSession(new { server = $"(address={localServerIpv4},priority=100)", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
        Assert.AreEqual(localServerIpv4, session.Settings.Server);
      }

      using (var session = MySQLX.GetSession(new { server = $"(address=server.example,priority=50),(address={localServerIpv4},priority=100)", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
        Assert.AreEqual(localServerIpv4, session.Settings.Server);
      }

      using (var session = MySQLX.GetSession(new { server = $"(address=server.example,priority=100),(address={localServerIpv4},priority=25),(address=192.0.10.56,priority=75)", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
        Assert.AreEqual(localServerIpv4, session.Settings.Server);
        Assert.AreEqual("server.example", FailoverManager.FailoverGroup.Hosts[0].Host);
        Assert.AreEqual("192.0.10.56", FailoverManager.FailoverGroup.Hosts[1].Host);
        Assert.AreEqual(localServerIpv4, FailoverManager.FailoverGroup.Hosts[2].Host);
      }

      using (var session = MySQLX.GetSession(new { host = $"(address={localServerIpv4},priority=2),(address=localhost,priority=3)", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
      }

      // Priority outside the 0-100 allowed range.
      Exception ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession(new { server = $"(address=server.example,priority=-20),(address={localServerIpv4},priority=100)", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }));
      Assert.AreEqual("The priority must be between 0 and 100.", ex.Message);

      ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession(new { server = $"(address=server.example,priority=-50),(address={localServerIpv4},priority=101)", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }));
      Assert.AreEqual("The priority must be between 0 and 100.", ex.Message);

      // Set priority for a subset of the hosts.
      ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession(new { server = $"(address=server.example),(address={localServerIpv4},priority=100)", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }));
      Assert.AreEqual("You must either assign no priority to any of the hosts or give a priority for every host.", ex.Message);
      ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession(new { server = $"(address=server.example,priority=50),(address={localServerIpv4})", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }));
      Assert.AreEqual("You must either assign no priority to any of the hosts or give a priority for every host.", ex.Message);
      ex = Assert.Throws<ArgumentException>(() => MySQLX.GetSession(new { server = $"(address=server.example,priority=50),(address={localServerIpv4},priority=100),(address=server.example)", port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }));
      Assert.AreEqual("You must either assign no priority to any of the hosts or give a priority for every host.", ex.Message);

      // Automatically set priority if no priority is given.
      string hostList = string.Empty;
      int priority = 100;
      for (int i = 1; i <= 105; i++)
      {
        hostList += "(address=server" + i + ".example,priority=" + (priority != 0 ? priority-- : 0) + "),";
        if (i == 105) hostList += "(address=localhost,priority=0)";
      }

      using (var session = MySQLX.GetSession(new { server = hostList, port = XPort, uid = uid, password = password, connecttimeout = connectionTimeout }))
      {
        Assert.AreEqual(SessionState.Open, session.InternalSession.SessionState);
        priority = 100;
        foreach (var host in FailoverManager.FailoverGroup.Hosts)
        {
          Assert.AreEqual(priority != 0 ? priority-- : 0, host.Priority);
        }
      }
    }

    #region WL14389

    [Test, Description("Test MySQLX Client Side Failover-Scenario-1(All Scenarios)")]
    public void FailoverConnectionScenarios()
    {
      if (!Platform.IsWindows()) Assert.Ignore("This test is for Windows OS only");
      if (!session.Version.isAtLeast(8, 0, 8)) Assert.Ignore("This test is for MySql 8.0.8 or higher");
      MySqlXConnectionStringBuilder sb = new MySqlXConnectionStringBuilder(ConnectionString);
      string ipV6Address = GetIPV6Address();

      //IP Address,IP Address:[xpluginport] with space in the connection string
      string connectionString = $"mysqlx://test:test@[{localServerIpv4},{localServerIpv4}:{sb.Port}]?connecttimeout=1000";
      isValidUri(connectionString);

      using (Session session1 = MySQLX.GetSession(connectionString))
      {
        var schema1 = session1.GetSchemas();
        Assert.IsNotNull(schema1);
      }

      //IP Address,IP Address:[xpluginport] without space in the connection string
      connectionString = $"mysqlx://test:test@[127.9.9.1,{localServerIpv4}:{sb.Port}]?ssl-mode=required";
      using (Session session1 = MySQLX.GetSession(connectionString))
      {
        var schema1 = session1.GetSchemas();
        Assert.IsNotNull(schema1);
      }

      connectionString = $"mysqlx://test:test@[143.24.20.36,{localServerIpv4}:{sb.Port}]?ssl-mode=required";
      using (Session session1 = MySQLX.GetSession(connectionString))
      {
        var schema1 = session1.GetSchemas();
        Assert.IsNotNull(schema1);
      }

      ////Single IP Address in an array in the connection string
      connectionString = $"mysqlx://test:test@[{sb.Server}:{sb.Port}]/test?ssl-mode=required";
      using (Session session1 = MySQLX.GetSession(connectionString))
      {
        var schema1 = session1.GetSchemas();
        Assert.IsNotNull(schema1);
      }

      ////Multiple Addresses which contains localhost:[xpluginport],[::1]:[xpluginport],IPV6 Adddress:[xpluginport]  in the connection string
      connectionString = $"mysqlx://test:test@[{Host}:{sb.Port},[{localServerIpv6}]:{sb.Port},{ipV6Address}:{sb.Port}]/test?ssl-mode=required";
      using (Session session1 = MySQLX.GetSession(connectionString))
      {
        var schema1 = session1.GetSchemas();
        Assert.IsNotNull(schema1);
      }

      ////Multiple Addresses which contains localhost:[xpluginport],localaddress:[33070],IPV6 Adddress:[xpluginport]  in the connection string with connection Timeout
      connectionString = $"mysqlx://test:test@[{sb.Server}:{sb.Port},{localServerIpv4}:{sb.Port},{ipV6Address}:{sb.Port}]/test?ssl-mode=required";
      using (Session session1 = MySQLX.GetSession(connectionString))
      {
        var schema1 = session1.GetSchemas();
        Assert.IsNotNull(schema1);
      }

      ////Multiple Addresses which contains invalidservername:[invalidport],localhost:[xpluginport]  in the connection string with connection Timeout
      connectionString = $"mysqlx://test:test@[invalidservername:9999,{sb.Server}:{sb.Port}]/test?ssl-mode=required";
      using (Session session1 = MySQLX.GetSession(connectionString))
      {
        var schema1 = session1.GetSchemas();
        Assert.IsNotNull(schema1);
      }

      ////Multiple Addresses which contains localhost:[invalidport],invalidlocaladdress:[xpluginport],localhost:[xpluginport]  in the connection string with connection Timeout
      connectionString = $"mysqlx://test:test@[{sb.Server}:9999,invalidlocaladdress:{sb.Port}]?ssl-mode=required&database=test";
      Assert.Catch(() => MySQLX.GetSession(connectionString));
    }

    /// <summary>
    ///   Bug 26198818
    /// </summary>
    [Test, Description("Test MySQLX Client Side Failover(Implicit Failover -Not supported)")]
    public void ImplicitFailover()
    {
      if (!Platform.IsWindows()) Assert.Ignore("This test is for Windows OS only");
      if (!session.Version.isAtLeast(8, 0, 8)) Assert.Ignore("This test is for MySql 8.0.8 or higher");
      MySqlXConnectionStringBuilder sb = new MySqlXConnectionStringBuilder(ConnectionString);
      string ipV6Address = GetIPV6Address();
      string connectionString = $"mysqlx://test:test@[{sb.Server},{localServerIpv4},{ipV6Address}:{sb.Port}]?implicit-failover";
      Assert.Catch(() => MySQLX.GetSession(connectionString + "&ssl-mode=required"));
    }

    [Test, Description("Provide three hosts to connection with one with priority and other two without priority")]
    public void HostsWithPriority()
    {
      if (!Platform.IsWindows()) Assert.Ignore("This test is for Windows OS only");
      if (!session.Version.isAtLeast(5, 7, 0)) Assert.Ignore("This test is for MySql 5.7 or higher");

      MySqlXConnectionStringBuilder sb = new MySqlXConnectionStringBuilder(ConnectionString);
      var connectionTimeout = 1;
      var address_priority1 = $"(address = {localServerIpv4}, priority = 2),(address =[{localServerIpv6}]),(address = localhost)";
      var address_priority2 = $"(address = {localServerIpv4}),(address =[{localServerIpv6}]),(address = localhost,priority = 100)";

      var connStr = $"mysqlx://test:test@[{address_priority1}]?ssl-mode=Required";
      Assert.Throws<ArgumentException>(() => MySQLX.GetSession(connStr));

      connStr = $"mysqlx://test:test@[{address_priority2}]?ssl-mode=Required";
      Assert.Throws<ArgumentException>(() => MySQLX.GetSession(connStr));

      connStr = $"server={address_priority1};port={sb.Port};uid={sb.UserID};password={sb.Password};connectiontimeout={connectionTimeout};ssl-mode=Required";
      Assert.Throws<ArgumentException>(() => MySQLX.GetSession(connStr));

      connStr = $"server={address_priority2};port={sb.Port};uid={sb.UserID};password={sb.Password};connectiontimeout={connectionTimeout};ssl-mode=Required";
      Assert.Throws<ArgumentException>(() => MySQLX.GetSession(connStr));

      Assert.Throws<ArgumentException>(() => MySQLX.GetSession(new
      {
        server = address_priority1,
        port = sb.Port,
        user = sb.UserID,
        password = string.Empty
      }));

      Assert.Throws<ArgumentException>(() => MySQLX.GetSession(new
      {
        server = address_priority2,
        port = sb.Port,
        user = sb.UserID,
        password = string.Empty
      }));
    }

    [Test, Description("Provide 101 hosts to connection without priority where 1st 100 hosts are invalid ones(Internal priority is set from 100...0) and " +
     "the last host is valid")]
    public void ManyInvalidHost()
    {
      if (!Platform.IsWindows()) Assert.Ignore("This test is for Windows OS only");
      if (!session.Version.isAtLeast(5, 7, 0)) Assert.Ignore("This test is for MySql 5.7 or higher");

      var connectionTimeout = 1;
      MySqlXConnectionStringBuilder sb = new MySqlXConnectionStringBuilder(ConnectionString);

      // Automatically set priority if no priority is given.
      var hostList = string.Empty;
      var priority = 100;
      for (var i = 1; i <= 101; i++)
      {
        hostList += "(address=server" + i + ".example,priority=" + (priority != 0 ? priority-- : 0) + "),";
        if (i == 101) hostList += $"(address={Host},priority=0)";
      }

      using (var session1 = MySQLX.GetSession("server=" + hostList + ";port=" + sb.Port + ";uid=" +
                                              sb.UserID + ";password=" + sb.Password + ";connect-timeout=" +
                                              connectionTimeout + ";ssl-mode=Required"))
      {
        Assert.AreEqual(SessionState.Open, session1.InternalSession.SessionState);
        var schema = session1.GetSchema("test");
        Assert.IsNotNull(schema);
        schema.DropCollection("test123");
        var testColl = schema.CreateCollection("test123");
        Assert.IsNotNull(testColl);
        schema.DropCollection("test123");
      }

      hostList = string.Empty;
      priority = 100;
      for (int i = 1; i <= 101; i++)
      {
        hostList += "(address=server" + i + ".example,priority=" + (priority != 0 ? priority-- : 0) + "),";
        if (i == 101) hostList += $"(address={Host}:" + sb.Port + ",priority=0)";
      }
      var connStr = "mysqlx://test:test@[" + hostList + "]" + "?ssl-mode=Required";
      using (var session1 = MySQLX.GetSession(connStr))
      {
        Assert.AreEqual(SessionState.Open, session1.InternalSession.SessionState);
        var schema = session1.GetSchema("test");
        Assert.IsNotNull(schema);
        schema.DropCollection("test123");
        var testColl = schema.CreateCollection("test123");
        Assert.IsNotNull(testColl);
        schema.DropCollection("test123");
      }

      hostList = string.Empty;
      priority = 100;
      for (var i = 1; i <= 101; i++)
      {
        hostList += "(address=server" + i + ".example,priority=" + (priority != 0 ? priority-- : 0) + "),";
        if (i == 101) hostList += $"(address={Host},priority=0)";
      }

      using (var session1 = MySQLX.GetSession(new
      {
        server = hostList,
        port = sb.Port,
        user = sb.UserID,
        password = sb.Password,
        sslmode = MySqlSslMode.None
      }))
      {
        Assert.AreEqual(SessionState.Open, session1.InternalSession.SessionState);
        var schema = session.GetSchema("test");
        Assert.IsNotNull(schema);
        schema.DropCollection("test123");
        var testColl = schema.CreateCollection("test123");
        Assert.IsNotNull(testColl);
        schema.DropCollection("test123");
      }
    }

    [Test, Description("Provide two hosts to connection with priority where both are valid")]
    public void TwoValidHost()
    {
      if (!Platform.IsWindows()) Assert.Ignore("This test is for Windows OS only");
      if (!session.Version.isAtLeast(5, 7, 0)) Assert.Ignore("This test is for MySql 5.7 or higher");
      MySqlXConnectionStringBuilder sb = new MySqlXConnectionStringBuilder(ConnectionString);

      var connStr = $"mysqlx://test:test@[ (address={localServerIpv4}:{sb.Password}, priority=0,address={Host}:{sb.Port}, priority=100)]?ssl-mode=Required";
      using (var sessionTest = MySQLX.GetSession(connStr))
      {
        Assert.AreEqual(SessionState.Open, sessionTest.InternalSession.SessionState);
      }

      var address_priority = $"(address = {localServerIpv4}, priority = 0),(address={Host}, priority=100)";

      connStr = "server=" + address_priority + ";port=" + sb.Port + ";uid=" + sb.UserID +
                ";password=" + sb.Password + ";ssl-mode=Required";
      using (var sessionTest = MySQLX.GetSession(connStr))
      {
        Assert.AreEqual(SessionState.Open, sessionTest.InternalSession.SessionState);
      }

      using (var sessionTest = MySQLX.GetSession(new
      {
        server = address_priority,
        port = sb.Port,
        user = sb.UserID,
        password = sb.Password,
        sslmode = MySqlSslMode.None
      }))
      {
        Assert.AreEqual(SessionState.Open, sessionTest.InternalSession.SessionState);
      }

    }

    [Test, Description("Provide two hosts to connection with priority where both are valid-with default port")]
    public void TwoValidHostWithDefaultPort()
    {
      if (!Platform.IsWindows()) Assert.Ignore("This test is for Windows OS only");
      if (!session.Version.isAtLeast(5, 7, 0)) Assert.Ignore("This test is for MySql 5.7 or higher");
      MySqlXConnectionStringBuilder sb = new MySqlXConnectionStringBuilder(ConnectionString);

      var connStr = "mysqlx://test:test"+
                    $"@[ (address={localServerIpv4}:{sb.Port}, priority=0,address={Host}:{sb.Port}, priority=100)]?ssl-mode=Required";

      using (var sessionTest = MySQLX.GetSession(connStr))
      {
        Assert.AreEqual(SessionState.Open, sessionTest.InternalSession.SessionState);
      }

      var address_priority = $"(address={localServerIpv4}, priority=0),(address={Host}, priority=100)";
      connStr = "server=" + address_priority + ";uid=" + sb.UserID + ";password=" + sb.Password +
                ";ssl-mode=Required;" + "port=" + sb.Port;
      using (var sessionTest = MySQLX.GetSession(connStr))
      {
        Assert.AreEqual(SessionState.Open, sessionTest.InternalSession.SessionState);
      }
      using (var sessionTest = MySQLX.GetSession(new
      {
        server = address_priority,
        user = sb.UserID,
        password = sb.Password,
        sslmode = MySqlSslMode.Required,
        port = sb.Port
      }))
      {
        Assert.AreEqual(SessionState.Open, sessionTest.InternalSession.SessionState);
      }
    }

    [Test, Description("Provide a single host to connection with priority and disconnect and connect again(iterate priority from 0 - 100) ")]
    public void IteratedPriority()
    {
      if (!Platform.IsWindows()) Assert.Ignore("This test is for Windows OS only");
      if (!session.Version.isAtLeast(5, 7, 0)) Assert.Ignore("This test is for MySql 5.7 or higher");
      var connectionTimeout = 900;

      // Automatically set priority if no priority is given.
      var hostList = new string[101];
      var hostListPort = new string[101];
      var priority = 100;
      for (var i = 0; i <= 100; i++)
      {
        hostList[i] = "(address=" + localServerIpv4 + ",priority=" + (priority != 0 ? priority-- : 0) + ")";
        var test = "server=" + hostList[i] + ";uid=test;password=test;connect-timeout=" + connectionTimeout + ";ssl-mode=required;" + "port=" + XPort;
        using (var session1 = MySQLX.GetSession(test))
        {
          Assert.AreEqual(SessionState.Open, session1.InternalSession.SessionState);
        }

        using (var session1 = MySQLX.GetSession("server=" + hostList[i] + ";port=" + XPort + ";uid=test;password=test;connect-timeout=" + connectionTimeout + ";ssl-mode=Required"))
        {
          Assert.AreEqual(SessionState.Open, session1.InternalSession.SessionState);
        }

        hostList[i] = "(address=" + localServerIpv4 + ":" + XPort + ",priority=" + (priority != 0 ? priority-- : 0) + ")";
        var connStr = "mysqlx://test:test@[" + hostList[i] + "]?ssl-mode=Required";
        using (var session1 = MySQLX.GetSession(connStr))
        {
          Assert.AreEqual(SessionState.Open, session1.InternalSession.SessionState);
        }

        hostListPort[i] = $"(address={Host}:{XPort},priority={(priority != 0 ? priority-- : 0)})";
        connStr = "mysqlx://test:test@[" + hostList[i] + "]?ssl-mode=Required";
        using (var session1 = MySQLX.GetSession(connStr))
        {
          Assert.AreEqual(SessionState.Open, session1.InternalSession.SessionState);
        }

        hostList[i] = "(address=" + localServerIpv4 + ",priority=" + (priority != 0 ? priority-- : 0) + ")";
        using (var session1 = MySQLX.GetSession(new
        {
          server = hostList[i],
          port = XPort,
          user = "test",
          password = "test",
          sslmode = MySqlSslMode.Required
        }))
        {
          Assert.AreEqual(SessionState.Open, session1.InternalSession.SessionState);
        }
      }
    }

    [Test, Description("Provide a single host to connection with priority with SSL")]
    public void PriorityWithSsl()
    {
      if (!Platform.IsWindows()) Assert.Ignore("This test is for Windows OS only");
      if (!session.Version.isAtLeast(5, 7, 0)) Assert.Ignore("This test is for MySql 5.7 or higher");
      var certificatePassword = "pass";
      var certificatewrongPassword = "wrongpass";

      var connStr = $"mysqlx://test:test@[ (address={localServerIpv4}:{XPort}, priority=0)]" +
                    $"/?ssl-mode=VerifyCA&ssl-ca-pwd={certificatePassword}&ssl-ca={sslCa}";

      using (var session1 = MySQLX.GetSession(connStr))
      {
        Assert.AreEqual(SessionState.Open, session1.InternalSession.SessionState);
      }

      connStr = $"mysqlx://test:test@[ (address={localServerIpv4}:{XPort}, priority=100)]" +
                $"/?ssl-mode=Required&ssl-ca-pwd={certificatePassword}&ssl-ca={sslCa}";
      using (var session1 = MySQLX.GetSession(connStr))
      {
        Assert.AreEqual(SessionState.Open, session1.InternalSession.SessionState);
      }

      connStr = $"mysqlx://test:test@[ (address={localServerIpv4}" + ":" + XPort + ", priority=100)]" +
                "/?ssl-mode=Required&ssl-ca-pwd=" + certificatePassword + "&ssl-ca=" + clientPfxIncorrect;
      Assert.Catch(()=> MySQLX.GetSession(connStr));

      var address_priority = $"(address = {localServerIpv4}, priority = 50 , address = localhost, priority = 100)";
      connStr = "server=" + address_priority + ";port=" + XPort + ";uid=test;password=test;ssl-mode=VerifyCA;ssl-ca-pwd=pass;ssl-ca=" + sslCa;
      using (var session1 = MySQLX.GetSession(connStr))
      {
        Assert.AreEqual(SessionState.Open, session1.InternalSession.SessionState);
      }

      address_priority = $"(address = {localServerIpv4}, priority = 25)";
      connStr = "server=" + address_priority + ";port=" + XPort + ";uid=test;password=test;ssl-mode=Required;ssl-ca-pwd=" + certificatePassword + ";ssl-ca=" + sslCa;
      using (var session1 = MySQLX.GetSession(connStr))
      {
        Assert.AreEqual(SessionState.Open, session1.InternalSession.SessionState);
      }

      connStr = "server=" + address_priority + ";port=" + XPort + ";uid=test;password=test;ssl-mode=VerifyFull;ssl-ca-pwd=" +
                certificatewrongPassword + ";ssl-ca=" + sslCa;
      Assert.Catch(() => MySQLX.GetSession(connStr));

      using (var session1 = MySQLX.GetSession(new
      {
        server = address_priority,
        port = XPort,
        user = "test",
        password = "test",
        sslmode = MySqlSslMode.VerifyCA,
        sslca = sslCa,
        CertificatePassword = certificatePassword
      }))
      {
        Assert.AreEqual(SessionState.Open, session1.InternalSession.SessionState);
      }

      using (var session1 = MySQLX.GetSession(new
      {
        server = address_priority,
        port = XPort,
        user = "test",
        password = "test",
        sslmode = MySqlSslMode.Required,
        sslca = sslCa,
        CertificatePassword = certificatePassword
      }))
      {
        Assert.AreEqual(SessionState.Open, session1.InternalSession.SessionState);
      }

        Assert.Catch(()=> MySQLX.GetSession(new
        {
          server = address_priority,
          port = XPort,
          user = "test",
          password = "test",
          sslmode = MySqlSslMode.VerifyFull,
          sslca = clientPfxIncorrect,
          CertificatePassword = certificatewrongPassword
        }));

    }

    /// <summary>
    /// Bug26524213
    /// </summary>
    [Test, Description("Provide a single host to connection with priority with SSL-Anonymous with VerifyFull SSL option")]
    public void SingleHostWithPriorityVerifyFull()
    {
      var address_priority = $"(address = {localServerIpv4}, priority = 50)";
      var certificatePassword = "pass";
      using (var session1 = MySQLX.GetSession(new
      {
        server = address_priority,
        port = XPort,
        user = session.Settings.UserID,
        password = session.Settings.Password,
        sslmode = MySqlSslMode.VerifyFull,
        CertificateFile = sslCa,
        sslCert = sslCert,
        sslkey = sslKey,
        CertificatePassword = certificatePassword
      }))
      {
        Assert.AreEqual(SessionState.Open,session1.InternalSession.SessionState);
      }
    }

    #endregion

    #region Methods
    internal void isValidUri(string inputUri)
    {
      try
      {
        using (var s1= MySQLX.GetSession(inputUri))
        {
        }
      }
      catch (Exception ex)
      {
        Assert.Fail($"localServerIpv4={localServerIpv4},localServerIpv6={localServerIpv6},cs={inputUri} " + ex.Message);
      }
    }
    #endregion Methods

  }
}
