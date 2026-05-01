<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="e7df3509-f7f2-4c6e-862d-5893f30e8426" Name="ControlStatus" Group="Custom">
	<Description>Статус контроля</Description>
	<SchemePhysicalColumn ID="c251147f-9ae7-4c7c-b634-12c1f04a2877" Name="ID" Type="Int16 Not Null" />
	<SchemePhysicalColumn ID="30f451ea-6009-49ea-b427-61727a926e64" Name="Name" Type="String(128) Not Null" />
	<SchemePrimaryKey ID="e178b727-1dd2-426a-9acb-82474ebbaeae" Name="pk_ControlStatus">
		<SchemeIndexedColumn Column="c251147f-9ae7-4c7c-b634-12c1f04a2877" />
	</SchemePrimaryKey>
	<SchemeRecord>
		<ID ID="c251147f-9ae7-4c7c-b634-12c1f04a2877">0</ID>
		<Name ID="30f451ea-6009-49ea-b427-61727a926e64">Нет контроля</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="c251147f-9ae7-4c7c-b634-12c1f04a2877">1</ID>
		<Name ID="30f451ea-6009-49ea-b427-61727a926e64">На контроле</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="c251147f-9ae7-4c7c-b634-12c1f04a2877">2</ID>
		<Name ID="30f451ea-6009-49ea-b427-61727a926e64">Снято с контроля</Name>
	</SchemeRecord>
</SchemeTable>