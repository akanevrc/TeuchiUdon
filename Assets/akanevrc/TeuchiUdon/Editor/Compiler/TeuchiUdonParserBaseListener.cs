//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.3
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from .\TeuchiUdonParser.g4 by ANTLR 4.9.3

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace akanevrc.TeuchiUdon.Editor.Compiler {

    #pragma warning disable 3021


using Antlr4.Runtime.Misc;
using IErrorNode = Antlr4.Runtime.Tree.IErrorNode;
using ITerminalNode = Antlr4.Runtime.Tree.ITerminalNode;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="ITeuchiUdonParserListener"/>,
/// which can be extended to create a listener which only needs to handle a subset
/// of the available methods.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.3")]
[System.Diagnostics.DebuggerNonUserCode]
[System.CLSCompliant(false)]
public partial class TeuchiUdonParserBaseListener : ITeuchiUdonParserListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.target"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterTarget([NotNull] TeuchiUdonParser.TargetContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.target"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitTarget([NotNull] TeuchiUdonParser.TargetContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.body"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBody([NotNull] TeuchiUdonParser.BodyContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.body"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBody([NotNull] TeuchiUdonParser.BodyContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>VarBindTopStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.topStatement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterVarBindTopStatement([NotNull] TeuchiUdonParser.VarBindTopStatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>VarBindTopStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.topStatement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitVarBindTopStatement([NotNull] TeuchiUdonParser.VarBindTopStatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>ExprTopStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.topStatement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExprTopStatement([NotNull] TeuchiUdonParser.ExprTopStatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>ExprTopStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.topStatement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExprTopStatement([NotNull] TeuchiUdonParser.ExprTopStatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.varBind"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterVarBind([NotNull] TeuchiUdonParser.VarBindContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.varBind"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitVarBind([NotNull] TeuchiUdonParser.VarBindContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>UnitVarDecl</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.varDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterUnitVarDecl([NotNull] TeuchiUdonParser.UnitVarDeclContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>UnitVarDecl</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.varDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitUnitVarDecl([NotNull] TeuchiUdonParser.UnitVarDeclContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>SingleVarDecl</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.varDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterSingleVarDecl([NotNull] TeuchiUdonParser.SingleVarDeclContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>SingleVarDecl</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.varDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitSingleVarDecl([NotNull] TeuchiUdonParser.SingleVarDeclContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>TupleVarDecl</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.varDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterTupleVarDecl([NotNull] TeuchiUdonParser.TupleVarDeclContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>TupleVarDecl</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.varDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitTupleVarDecl([NotNull] TeuchiUdonParser.TupleVarDeclContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.qualified"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterQualified([NotNull] TeuchiUdonParser.QualifiedContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.qualified"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitQualified([NotNull] TeuchiUdonParser.QualifiedContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="TeuchiUdonParser.identifier"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterIdentifier([NotNull] TeuchiUdonParser.IdentifierContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="TeuchiUdonParser.identifier"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitIdentifier([NotNull] TeuchiUdonParser.IdentifierContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>ReturnUnitStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterReturnUnitStatement([NotNull] TeuchiUdonParser.ReturnUnitStatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>ReturnUnitStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitReturnUnitStatement([NotNull] TeuchiUdonParser.ReturnUnitStatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>ReturnValueStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterReturnValueStatement([NotNull] TeuchiUdonParser.ReturnValueStatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>ReturnValueStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitReturnValueStatement([NotNull] TeuchiUdonParser.ReturnValueStatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>ContinueUnitStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterContinueUnitStatement([NotNull] TeuchiUdonParser.ContinueUnitStatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>ContinueUnitStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitContinueUnitStatement([NotNull] TeuchiUdonParser.ContinueUnitStatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>ContinueValueStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterContinueValueStatement([NotNull] TeuchiUdonParser.ContinueValueStatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>ContinueValueStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitContinueValueStatement([NotNull] TeuchiUdonParser.ContinueValueStatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>BreakUnitStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBreakUnitStatement([NotNull] TeuchiUdonParser.BreakUnitStatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>BreakUnitStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBreakUnitStatement([NotNull] TeuchiUdonParser.BreakUnitStatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>BreakValueStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBreakValueStatement([NotNull] TeuchiUdonParser.BreakValueStatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>BreakValueStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBreakValueStatement([NotNull] TeuchiUdonParser.BreakValueStatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>LetBindStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterLetBindStatement([NotNull] TeuchiUdonParser.LetBindStatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>LetBindStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitLetBindStatement([NotNull] TeuchiUdonParser.LetBindStatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>ExprStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExprStatement([NotNull] TeuchiUdonParser.ExprStatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>ExprStatement</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExprStatement([NotNull] TeuchiUdonParser.ExprStatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>EvalVarExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterEvalVarExpr([NotNull] TeuchiUdonParser.EvalVarExprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>EvalVarExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitEvalVarExpr([NotNull] TeuchiUdonParser.EvalVarExprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>AccessExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAccessExpr([NotNull] TeuchiUdonParser.AccessExprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>AccessExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAccessExpr([NotNull] TeuchiUdonParser.AccessExprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>EvalUnitFuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterEvalUnitFuncExpr([NotNull] TeuchiUdonParser.EvalUnitFuncExprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>EvalUnitFuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitEvalUnitFuncExpr([NotNull] TeuchiUdonParser.EvalUnitFuncExprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>EvalTupleFuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterEvalTupleFuncExpr([NotNull] TeuchiUdonParser.EvalTupleFuncExprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>EvalTupleFuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitEvalTupleFuncExpr([NotNull] TeuchiUdonParser.EvalTupleFuncExprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>ValueBlockExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterValueBlockExpr([NotNull] TeuchiUdonParser.ValueBlockExprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>ValueBlockExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitValueBlockExpr([NotNull] TeuchiUdonParser.ValueBlockExprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>UnitBlockExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterUnitBlockExpr([NotNull] TeuchiUdonParser.UnitBlockExprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>UnitBlockExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitUnitBlockExpr([NotNull] TeuchiUdonParser.UnitBlockExprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>LiteralExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterLiteralExpr([NotNull] TeuchiUdonParser.LiteralExprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>LiteralExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitLiteralExpr([NotNull] TeuchiUdonParser.LiteralExprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>LetInBindExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterLetInBindExpr([NotNull] TeuchiUdonParser.LetInBindExprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>LetInBindExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitLetInBindExpr([NotNull] TeuchiUdonParser.LetInBindExprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>ParenExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterParenExpr([NotNull] TeuchiUdonParser.ParenExprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>ParenExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitParenExpr([NotNull] TeuchiUdonParser.ParenExprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>FuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFuncExpr([NotNull] TeuchiUdonParser.FuncExprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>FuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFuncExpr([NotNull] TeuchiUdonParser.FuncExprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>EvalSingleFuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterEvalSingleFuncExpr([NotNull] TeuchiUdonParser.EvalSingleFuncExprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>EvalSingleFuncExpr</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.expr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitEvalSingleFuncExpr([NotNull] TeuchiUdonParser.EvalSingleFuncExprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>IntegerLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterIntegerLiteral([NotNull] TeuchiUdonParser.IntegerLiteralContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>IntegerLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitIntegerLiteral([NotNull] TeuchiUdonParser.IntegerLiteralContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>HexIntegerLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterHexIntegerLiteral([NotNull] TeuchiUdonParser.HexIntegerLiteralContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>HexIntegerLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitHexIntegerLiteral([NotNull] TeuchiUdonParser.HexIntegerLiteralContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>BinIntegerLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBinIntegerLiteral([NotNull] TeuchiUdonParser.BinIntegerLiteralContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>BinIntegerLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBinIntegerLiteral([NotNull] TeuchiUdonParser.BinIntegerLiteralContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>RealLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterRealLiteral([NotNull] TeuchiUdonParser.RealLiteralContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>RealLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitRealLiteral([NotNull] TeuchiUdonParser.RealLiteralContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>CharacterLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCharacterLiteral([NotNull] TeuchiUdonParser.CharacterLiteralContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>CharacterLiteral</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCharacterLiteral([NotNull] TeuchiUdonParser.CharacterLiteralContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>RegularString</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterRegularString([NotNull] TeuchiUdonParser.RegularStringContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>RegularString</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitRegularString([NotNull] TeuchiUdonParser.RegularStringContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>VervatiumString</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterVervatiumString([NotNull] TeuchiUdonParser.VervatiumStringContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>VervatiumString</c>
	/// labeled alternative in <see cref="TeuchiUdonParser.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitVervatiumString([NotNull] TeuchiUdonParser.VervatiumStringContext context) { }

	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void EnterEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void ExitEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitTerminal([NotNull] ITerminalNode node) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitErrorNode([NotNull] IErrorNode node) { }
}
} // namespace akanevrc.TeuchiUdon.Editor.Compiler
