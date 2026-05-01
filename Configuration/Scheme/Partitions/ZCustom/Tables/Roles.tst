<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="81f6010b-9641-4aa5-8897-b8e8603fbf4b" Partition="29f90c69-c1ef-4cbf-b9d5-7fc91cd68c67">
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="7cc2ba65-c921-4acc-b93a-93d188db777f" Name="Head" Type="Reference(Typified) Null" ReferencedTable="81f6010b-9641-4aa5-8897-b8e8603fbf4b">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="7cc2ba65-c921-00cc-4000-03d188db777f" Name="HeadID" Type="Guid Null" ReferencedColumn="81f6010b-9641-01a5-4000-08e8603fbf4b" />
		<SchemeReferencingColumn ID="248e852c-2326-4516-9276-c23f06559f1f" Name="HeadName" Type="String(128) Null" ReferencedColumn="616d6b2e-35d5-424d-846b-618eb25962d0" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="0752cd97-365f-4e01-9398-d36619fea77c" Name="IndexDistrict" Type="String(128) Null">
		<Description>Поле для указания индекса района</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="7506cef0-4e79-4b06-a998-9c110a487049" Name="ExternalID2" Type="String(128) Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="b3639e20-4571-467c-817a-2c36554a0bce" Name="ExternalID3" Type="String(128) Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="b85fb055-2e9c-4eca-a39f-56012e688b53" Name="IndexDep" Type="String(128) Null">
		<Description>Поле для указания индекса подразделения</Description>
	</SchemePhysicalColumn>
	<Predicate Dbms="SqlServer">[TypeID] &lt;&gt; 6</Predicate>
	<Predicate Dbms="PostgreSql">"TypeID" &lt;&gt; 6</Predicate>
	<Predicate Dbms="SqlServer">[AdSyncID] IS NOT NULL</Predicate>
	<Predicate Dbms="PostgreSql">"AdSyncID" IS NOT NULL</Predicate>
</SchemeTable>