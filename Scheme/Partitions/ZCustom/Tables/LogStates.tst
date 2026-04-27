<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="edab83a5-7559-44d9-b13b-82215d44d63b" Name="LogStates" Group="Custom">
	<SchemePhysicalColumn ID="6f1c4d4b-04b8-4a98-9529-12bc8082cf11" Name="ID" Type="Int16 Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="f8ac0e0d-f300-4d10-85b4-5c1dea267c0c" Name="df_LogStates_ID" Value="0" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="ee17819c-baba-4ad3-81f2-4972233861a6" Name="Name" Type="String(128) Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="f5c56eae-0240-42d4-8248-73df0ec89f69" Name="df_LogStates_Name" Value="Не начато" />
	</SchemePhysicalColumn>
	<SchemePrimaryKey ID="95cf4142-cefb-447a-a26c-547c8ee8105a" Name="pk_LogStates">
		<SchemeIndexedColumn Column="6f1c4d4b-04b8-4a98-9529-12bc8082cf11" />
	</SchemePrimaryKey>
	<SchemeRecord>
		<ID ID="6f1c4d4b-04b8-4a98-9529-12bc8082cf11">0</ID>
		<Name ID="ee17819c-baba-4ad3-81f2-4972233861a6">Не начато</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="6f1c4d4b-04b8-4a98-9529-12bc8082cf11">1</ID>
		<Name ID="ee17819c-baba-4ad3-81f2-4972233861a6">Рассмотрен</Name>
	</SchemeRecord>
	<SchemeRecord>
		<ID ID="6f1c4d4b-04b8-4a98-9529-12bc8082cf11">2</ID>
		<Name ID="ee17819c-baba-4ad3-81f2-4972233861a6">Исполнено</Name>
	</SchemeRecord>
</SchemeTable>