
1. Repository 与 UnitOfWork 的关系？
		Repository 关注的是数据处理的细节问题，如增删改查；而 UnitOfWork 关注的是连接、事物等的数据库方面的问题。
			关系图：	
				Repository		——>	Data/CRUD
				UnitOfWork	——>	DB/Connection
		1）两者间是不同角度的关系，而不是完全的包含关系；
		2）DAL 层的处理过程都是建立在 DbConnection 之上的，所以 Repository 类需要以 UnitOfWork 为成员；

2. 如何配置 IDAL 工具类？
		通过上面的包含关系，可知 DAL 层的配置需要
			1）初始化注入 UnitOfWork 类；
			2）BLL 层需要继承 Repository 类。
	
3. 具体配置，需要导入的 DLL 工具？
		DAL 层：
			1）导入 Dapper.dll ；
			2）导入 MySql.Data.dll ；
		UI 层：
			1）Unity 4 个 dll；
			2）Boostrapper.cs 引导并注入 IUnitOfWork；






















