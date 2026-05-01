<?xml version="1.0" encoding="utf-8"?>
<SchemeTable IsSystem="true" IsPermanent="true" IsSealed="true" ID="c4fcd8d3-fcb1-451f-98f4-e352cd8a3a41" Name="Scheme" Group="System">
	<Description>Scheme properties</Description>
	<SchemePhysicalColumn ID="7ae3e957-e0d2-441b-af4f-a0f6f365648c" Name="ID" Type="Guid Not Null" IsRowGuidColumn="true">
		<Description>Scheme ID</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="980ce038-538d-454e-8c77-716f5ef5d90e" Name="Name" Type="String(128) Not Null">
		<Description>Scheme name</Description>
		<Collation Dbms="SqlServer">Latin1_General_BIN2</Collation>
		<Collation Dbms="PostgreSql">POSIX</Collation>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="64b0efb9-8434-4566-b0bb-5406bb3b0c39" Name="Description" Type="String(Max) Null">
		<Description>Scheme description</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="241f405c-ab80-4adb-8cc3-873632a52018" Name="CollationSqlServer" Type="String(Max) Null">
		<Description>$Scheme_Descriptions_Scheme_CollationSqlServer(en)</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="774a5531-8d0d-4547-8866-e42a3d5d54ea" Name="CollationPostgreSql" Type="String(Max) Null">
		<Description>$Scheme_Descriptions_Scheme_CollationPostgreSql(en)</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="0622ea8c-59c6-4d34-b6a0-4afbfb006e9a" Name="SchemeVersion" Type="String(15) Not Null">
		<Description>$Scheme_Descriptions_Scheme_SchemeVersion(en)</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="124df9e1-67c3-4051-904b-09eeb7259515" Name="Modified" Type="DateTime Not Null">
		<Description>Date/time of the last changes</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="fb198012-068e-459c-a5fa-87d36e883e36" Name="ModifiedBy" Type="Reference(Typified) Not Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3" WithForeignKey="false">
		<Description>Employee who made recent changes</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="fb198012-068e-009c-4000-07d36e883e36" Name="ModifiedByID" Type="Guid Not Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="1388d520-3229-42f9-8ad4-b9b70a4381d7" Name="ModifiedByName" Type="String(128) Not Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePrimaryKey ID="dcc6eb2a-d7a6-418f-a7b0-64092c3f804e" Name="pk_Scheme" IsClustered="true">
		<SchemeIndexedColumn Column="7ae3e957-e0d2-441b-af4f-a0f6f365648c" />
	</SchemePrimaryKey>
	<SchemeUniqueKey ID="247374c4-b6ca-411c-a788-bf9606d5a840" Name="ndx_Scheme_Name">
		<SchemeIndexedColumn Column="980ce038-538d-454e-8c77-716f5ef5d90e" />
	</SchemeUniqueKey>
</SchemeTable>