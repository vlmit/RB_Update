<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="ef4bbb91-4d48-4c68-9e05-34ab4d5c2b36" Name="FunctionRolesVirtual" Group="System" IsVirtual="true" InstanceType="Cards" ContentType="Entries">
	<Description>Виртуальная карточка для функциональной роли.</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="ef4bbb91-4d48-0068-2000-04ab4d5c2b36" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="ef4bbb91-4d48-0168-4000-04ab4d5c2b36" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="5357657e-2a03-4cda-9558-dd6cfb7bb99a" Name="FunctionRoleID" Type="Guid Not Null">
		<Description>Идентификатор функциональной роли.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="3667ef0b-5efb-4dfc-b9d2-bb7cc9d20c64" Name="Name" Type="String(128) Not Null">
		<Description>Имя функциональной роли.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="cdf0fc4e-f5bf-45a1-8cb0-acd12ea0aaf2" Name="Caption" Type="String(128) Not Null">
		<Description>Отображаемое пользователю имя функциональной роли.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="a69fb995-d1f2-4d68-852e-192b8b322882" Name="CanBeDeputy" Type="Boolean Not Null">
		<Description>Признак того, что при проверке вхождения в функциональную роль также проверяются заместители.</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="00d08eb2-928f-4410-a684-2f6badf6399d" Name="df_FunctionRolesVirtual_CanBeDeputy" Value="true" />
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="7110af74-feb7-4c08-8899-170db4ce0568" Name="Partition" Type="Reference(Typified) Not Null" ReferencedTable="5ca00fac-d04e-4b82-8139-9778518e00bf">
		<Description>Библиотека схемы, в которую включается функциональная роль.</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="7110af74-feb7-0008-4000-070db4ce0568" Name="PartitionID" Type="Guid Not Null" ReferencedColumn="fc636796-f583-4306-ad69-30fb2a070f9a" />
		<SchemeReferencingColumn ID="d9d6ebfc-37e8-47a2-b256-c61e430666e1" Name="PartitionName" Type="String(128) Not Null" ReferencedColumn="6af8d64d-cff0-4ece-9db3-b1f38e73814d" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="ef4bbb91-4d48-0068-5000-04ab4d5c2b36" Name="pk_FunctionRolesVirtual" IsClustered="true">
		<SchemeIndexedColumn Column="ef4bbb91-4d48-0168-4000-04ab4d5c2b36" />
	</SchemePrimaryKey>
</SchemeTable>