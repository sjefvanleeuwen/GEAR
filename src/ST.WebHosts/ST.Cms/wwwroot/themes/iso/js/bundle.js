!function (t) { var e = {}; function n(o) { if (e[o]) return e[o].exports; var i = e[o] = { i: o, l: !1, exports: {} }; return t[o].call(i.exports, i, i.exports, n), i.l = !0, i.exports } n.m = t, n.c = e, n.d = function (t, e, o) { n.o(t, e) || Object.defineProperty(t, e, { enumerable: !0, get: o }) }, n.r = function (t) { "undefined" != typeof Symbol && Symbol.toStringTag && Object.defineProperty(t, Symbol.toStringTag, { value: "Module" }), Object.defineProperty(t, "__esModule", { value: !0 }) }, n.t = function (t, e) { if (1 & e && (t = n(t)), 8 & e) return t; if (4 & e && "object" == typeof t && t && t.__esModule) return t; var o = Object.create(null); if (n.r(o), Object.defineProperty(o, "default", { enumerable: !0, value: t }), 2 & e && "string" != typeof t) for (var i in t) n.d(o, i, function (e) { return t[e] }.bind(null, i)); return o }, n.n = function (t) { var e = t && t.__esModule ? function () { return t.default } : function () { return t }; return n.d(e, "a", e), e }, n.o = function (t, e) { return Object.prototype.hasOwnProperty.call(t, e) }, n.p = "", n(n.s = 0) }([function (t, e, n) { "use strict"; n.r(e); class o { constructor() { this.setBurgerIcon(), this.modifyBurgerIconWhenToggleSideMenu() } setBurgerIcon() { const t = $("aside"), e = $("#isodms-logo"), n = t.hasClass("collapsed") ? "arrow_forward" : "arrow_back"; e.text(n) } modifyBurgerIconWhenToggleSideMenu() { $("#isodms-logo").bind("click", () => { const t = $("aside"), e = $("#isodms-logo"), n = t.hasClass("collapsed") ? "arrow_back" : "arrow_forward"; e.text(n) }) } } class i { constructor() { this.collapseAllExpandedCells(), this.bindToInlineEditingCell() } mutateCell(t) { if (!t.hasClass("expanded-cell")) { const e = t.outerWidth(), n = 2 * e; let o; o = e > 440 ? e : n > 440 ? 440 : n; const i = 3 * t.outerHeight(), a = i > 256 ? 256 : i; t.addClass("expanded-cell"), t.css("width", o + "px"), t.css("height", a + "px") } } revertCell(t) { t.hasClass("expanded-cell") && (t.removeClass("expanded-cell"), t.css("width", "inherit"), t.css("height", "inherit")) } bindToInlineEditingCell() { this.collapseAllExpandedCells(), $(".expandable-cell").each((t, e) => { const n = $(e), o = n.find("textarea"); $(o).on("mouseup", t => { 0 === t.button && this.mutateCell(n) }), $(o).on("blur", () => { this.revertCell(n) }) }) } collapseAllExpandedCells() { $("expanded-cell").each((t, e) => { $(e).removeClass("expanded-cell") }) } } class a { constructor(t, e, n) { const o = t, i = this.createInfoTooltipContentContainer(e); this.createTooltip(i, o, n), this.removeWhenMouseNotOver(o, i) } createTooltip(t, e, n) { document.body.appendChild(t), this.popper = new Popper(e, t, n) } removeWhenMouseNotOver(t, e) { $(document).on("mouseover", n => { n.target !== t && e.remove() }) } createInfoTooltipContentContainer(t) { const e = document.createElement("div"); return e.classList = ["info-tooltip-content p-4"], e.innerText = t, e } } class r { constructor() { this.bindToInfoTooltipClass() } bindToInfoTooltipClass() { $(document).on("mouseover", function (t) { $(t.target).hasClass("info-tooltip") && new a(t.target, $(t.target).data("content"), { placement: "bottom-start" }) }) } } class s { constructor() { this.bindToCollapseControls(), this.bindToCollapseParentButton(), function () { const t = $('[data-toggle="collapse"]'); $(t).each((t, e) => { const n = $(e).attr("data-target"); $(n).on("hide.bs.collapse", t => { !function t(e) { const n = $(e); const o = n.find('[data-toggle="collapse"]'); o && 0 !== o.length && o.each((e, n) => { const o = $(n).attr("data-target"); t(n), $(o).collapse("hide") }) }(t.target) }) }) }(), $('[data-collapse-linked="true"]').each((t, e) => { const n = $(e); n.on("show.bs.collapse", t => { !function (t) { t = $(t), $('[data-collapse-linked="true"]').each((e, n) => { const o = $(n); o !== t && o.collapse("hide") }) }(t.target) }) }) } static expandCollapse(t) { $(t).each((t, e) => { $(e).find(".collapse").first().collapse("show") }) } bindToCollapseControls() { $("#collapse-all").on("click", () => { $(".collapse").each((t, e) => { const n = $(e); n.hasClass("spoiler-collapse") || n.collapse("show") }) }), $("#hide-all").on("click", () => { $(".collapse").each((t, e) => { const n = $(e); n.hasClass("spoiler-collapse") || n.collapse("hide") }) }) } bindToCollapseParentButton() { $(".collapse-parent").each((t, e) => { $(e).on("click", () => { $(e).parents(".collapse").first().collapse("hide") }) }) } } var l, c = function () { function t(t, e, n) { this.target = t, this.element = e, this.positioningOptions = n, this.container = e, this.componentAppending(), this.componentRemoving() } return t.prototype.componentAppending = function () { return $(document.body).append(this.element), this.popper = new Popper(this.target, this.container, this.positioningOptions), this.target }, t.prototype.componentRemoving = function () { var t = this; $(document).on("mouseup", function (e) { $(t.container).is(e.target) || 0 !== $(t.container).has(e.target).length || (t.container.remove(), t.element = null, t.popper = null) }) }, t }(), d = function () { function t(t) { this.componentOptions = t, this.addButton = null, this.container = this.getContainer(), this.searchBar = this.getSearchBar(), this.componentBody = this.getComponentBody() } return t.prototype.getSearchBar = function () { return $('<div class="search-bar">\n                            <input type="text" placeholder="' + this.componentOptions.searchBarPlaceholder + '">\n                            <span class="material-icons bg-transparent">search</span>\n                        </div>')[0] }, t.prototype.getComponentBody = function () { return $('<div class="dynamic-select-body"></div>')[0] }, t.prototype.getAddButton = function () { return $('<div class="add-item">\n                    <div class="add-container">\n                        ' + this.componentOptions.addButtonLabel + '\n                    </div>\n                    <span class="material-icons">add</span>\n                </div>')[0] }, t.prototype.getContainer = function () { return $('<div class="dynamic-select"></div>')[0] }, t }(), u = (l = function (t, e) { return (l = Object.setPrototypeOf || { __proto__: [] } instanceof Array && function (t, e) { t.__proto__ = e } || function (t, e) { for (var n in e) e.hasOwnProperty(n) && (t[n] = e[n]) })(t, e) }, function (t, e) { function n() { this.constructor = t } l(t, e), t.prototype = null === e ? Object.create(e) : (n.prototype = e.prototype, new n) }), p = function (t) { function e(e, n, o) { var i = t.call(this, n) || this; return i.isThereAnExactMatchForItem = null, i.valuesArray = e, i.actionPromises = o, i.items = i.getItems(), i } return u(e, t), e.prototype.assemble = function () { var t = this; this.container.appendChild(this.searchBar), this.items.forEach(function (e) { t.componentBody.appendChild(e) }), this.container.appendChild(this.componentBody) }, e.prototype.handleActions = function () { var t = this; this.handleSearchInput(), this.items.forEach(function (e, n) { t.handleItemEvents(e, n) }) }, e.prototype.getComponent = function () { return this.assemble(), this.handleActions(), this.container }, e.prototype.handleItemSelect = function (t) { var e = this; t.on("click", function (n) { var o, i = $(n.target); if ((i.is(".item") || i.is("input")) && t.find("input:text").length) { if (t.hasClass("editing-item")) return; if (e.currentlyEditedItem) return void e.revertCurrentlyEditedItem(); o = t.attr("data-id"), e.emitSelectedValue(o), e.container.remove() } }) }, e.prototype.handleItemEvents = function (t, e) { var n = $(t), o = n.find("input").first(), i = n.find('[data-role="edit"]').first(), a = n.find('[data-role="accept"]').first(), r = n.find('[data-role="delete"]').first(), s = n.find('[data-role="undo"]').first(); this.handleItemDelete(r, e, n), this.handleItemEdit({ editButton: i, acceptButton: a, deleteButton: r, undoButton: s }, o, n), this.handleItemSave({ editButton: i, acceptButton: a, deleteButton: r, undoButton: s }, o, e, n), this.handleItemRevert(s), this.handleItemSelect(n) }, e.prototype.handleItemDelete = function (t, e, n) { var o = this; t.on("click", function () { o.disableItemEditing(), n.addClass("deleting-item"), o.actionPromises.delete(o.valuesArray[e], o.componentOptions).then(function () { n[0].remove() }).catch(function () { n.removeClass("deleting-item") }) }) }, e.prototype.handleItemSave = function (t, e, n, o) { var i = this; t.acceptButton.on("click", function () { o.addClass("saving-item"); var a = { id: i.valuesArray[n].id, value: e.val().toString().trim() }; i.actionPromises.update(a, i.componentOptions).then(function () { i.disableItemEditing(), t.acceptButton.blur(), i.inputInitialValue = null }).catch(function () { i.enableItemEditing(o, e) }).finally(function () { o.removeClass("saving-item") }) }) }, e.prototype.handleItemRevert = function (t) { var e = this; t.on("click", function () { e.revertCurrentlyEditedItem(), t.blur() }) }, e.prototype.handleSearchInput = function () { var t = this; $(this.searchBar).on("keyup", function (e) { var n = $(e.currentTarget).find("input"), o = n.val().toString().trim().toLowerCase(), i = $(t.componentBody); t.isThereAnExactMatchForItem = !1; var a = 0; i.find(".item:not(.add-item)").filter(function (t, e) { var n = $(e), i = n.find("input:text").val(), r = i.toString().toLowerCase().indexOf(o) > -1; return i.toString().toLowerCase() === o && a++ , r ? n.removeClass("display-none") : n.addClass("display-none"), null }), t.isThereAnExactMatchForItem = !!a, t.appendAddButton(n, o) }) }, e.prototype.handleActionAdd = function (t) { var e = this; if (this.addButton) { var n = $(this.addButton); n.on("click", function () { var o = t.val().toString().toLowerCase().trim(); e.actionPromises.create(o, e.componentOptions).then(function (t) { var i = e.getItem({ id: t, value: o, checked: !1 }), a = { id: t, value: o, checked: !1 }, r = e.items.push(i); n.addClass("saving-item"), e.valuesArray.push(a), e.componentBody.appendChild(i), e.handleItemEvents(i, r), e.addButton.remove() }).catch(function () { return n.removeClass("saving-item") }) }) } }, e.prototype.revertCurrentlyEditedItem = function () { var t = $(this.currentlyEditedItem).find("input"); this.inputInitialValue && t.val(this.inputInitialValue), this.disableItemEditing() }, e.prototype.disableItemEditing = function () { if (this.currentlyEditedItem) { var t = this.currentlyEditedItem.find("input").first(); t.blur(), document.getSelection().removeAllRanges(), t.attr("readonly", "readonly"), this.currentlyEditedItem.removeClass("editing-item"), this.currentlyEditedItem = null } }, e.prototype.emitSelectedValue = function (t) { $(this.container).trigger("selectValueChange", { value: t, options: this.componentOptions }) }, e.prototype.getItem = function (t) { var e = this.actionPromises.update ? '<button data-role="edit" class="mr-1"><span class="material-icons">edit</span></button>\n               <button data-role="accept" class="editing-only mr-1" style="display: none;">\n                   <span class="material-icons">checked</span>\n               </button>\n               <button data-role="undo" class="editing-only" style="display: none;">\n                   <span class="material-icons">replay</span>\n               </button>' : "", n = this.actionPromises.delete ? '<button data-role="delete"><span class="material-icons">clear</span></button>' : "", o = '<div class="item" data-id="' + t.id + '">\n                    <input type="text" style="width: 80%;" readonly value="' + t.value + '">\n                    ' + e + "\n                    " + n + "\n                    </div>"; return $(o)[0] }, e.prototype.getItems = function () { var t = this, e = []; return this.valuesArray.forEach(function (n) { var o = t.getItem(n); e.push(o) }), e }, e.prototype.appendAddButton = function (t, e) { this.actionPromises.create && (!this.isThereAnExactMatchForItem && e.trim().length ? this.addButton || (this.addButton = this.getAddButton(), this.componentBody.prepend(this.addButton), this.handleActionAdd(t)) : null !== this.addButton && (this.addButton.remove(), this.addButton = null)) }, e.prototype.handleItemEdit = function (t, e, n) { var o = this; t.editButton.on("click", function () { o.inputInitialValue = e.val().toString(), o.enableItemEditing(n, e) }) }, e.prototype.enableItemEditing = function (t, e) { this.disableItemEditing(), this.currentlyEditedItem = t, t.addClass("editing-item"), e.removeAttr("readonly"), e.select() }, e }(d), h = function () { var t = function (e, n) { return (t = Object.setPrototypeOf || { __proto__: [] } instanceof Array && function (t, e) { t.__proto__ = e } || function (t, e) { for (var n in e) e.hasOwnProperty(n) && (t[n] = e[n]) })(e, n) }; return function (e, n) { function o() { this.constructor = e } t(e, n), e.prototype = null === n ? Object.create(n) : (o.prototype = n.prototype, new o) } }(), m = function (t) { function e(e, n, o) { var i = t.call(this, n) || this; return i.isThereAnExactMatchForItem = null, i.valuesArray = e, i.actionPromises = o, i.items = i.getItems(), i } return h(e, t), e.prototype.assemble = function () { var t = this; this.container.appendChild(this.searchBar), this.items.forEach(function (e) { t.componentBody.appendChild(e) }), this.container.appendChild(this.componentBody) }, e.prototype.handleActions = function () { var t = this; this.handleSearchInput(), this.items.forEach(function (e, n) { t.handleItemEvents(e, n) }) }, e.prototype.getComponent = function () { return this.assemble(), this.handleActions(), this.container }, e.prototype.triggerContainerEvent = function (t, e) { $(this.container).trigger(t, { value: e, options: this.componentOptions }) }, e.prototype.handleItemEvents = function (t, e) { var n = $(t), o = n.find("input:text"), i = n.find("input:checkbox"), a = n.find('[data-role="edit"]').first(), r = n.find('[data-role="accept"]').first(), s = n.find('[data-role="delete"]').first(), l = n.find('[data-role="undo"]').first(); this.handleItemDelete(s, e, n), this.handleItemEdit({ editButton: a, acceptButton: r, deleteButton: s, undoButton: l }, o, i, n), this.handleItemSave({ editButton: a, acceptButton: r, deleteButton: s, undoButton: l }, o, i, e, n), this.handleItemRevert(l), this.handleItemSelect(n) }, e.prototype.handleItemDelete = function (t, e, n) { var o = this; t.on("click", function () { o.disableItemEditing(), n.addClass("deleting-item"), o.actionPromises.delete(o.valuesArray[e], o.componentOptions).then(function () { n[0].remove() }).catch(function () { n.removeClass("deleting-item") }) }) }, e.prototype.handleItemSave = function (t, e, n, o, i) { var a = this; t.acceptButton.on("click", function () { i.addClass("saving-item"); var r = { id: a.valuesArray[o].id, value: e.val().toString().trim() }; a.actionPromises.update(r, a.componentOptions).then(function () { a.disableItemEditing(), t.acceptButton.blur(), a.inputInitialValue = null }).catch(function () { a.enableItemEditing(i, e, n) }).finally(function () { i.removeClass("saving-item") }) }) }, e.prototype.handleItemRevert = function (t) { var e = this; t.on("click", function () { e.revertCurrentlyEditedItem(), t.blur() }) }, e.prototype.handleItemSelect = function (t) { var e = this; t.on("click", function (n) { var o = t.find("input:checkbox"), i = $(n.target); i.is(".item") || i.parent(".item").length ? (e.currentlyEditedItem && e.revertCurrentlyEditedItem(), o.prop("checked", !o.prop("checked")), e.emmitValueChangeEvent(t, o)) : i.is("input:checkbox") && (e.currentlyEditedItem && e.revertCurrentlyEditedItem(), e.emmitValueChangeEvent(t, o)) }) }, e.prototype.emmitValueChangeEvent = function (t, e) { this.triggerContainerEvent("selectValueChange", { changedValue: { id: t.data("id"), checked: e.prop("checked") }, options: this.componentOptions }) }, e.prototype.handleSearchInput = function () { var t = this; $(this.searchBar).on("keyup", function (e) { var n = $(e.currentTarget).find("input"), o = n.val().toString().trim().toLowerCase(), i = $(t.componentBody); t.isThereAnExactMatchForItem = !1; var a = 0; i.find(".item:not(.add-item)").filter(function (t, e) { var n = $(e), i = n.find("input:text").val(), r = i.toString().toLowerCase().indexOf(o) > -1; return i.toString().toLowerCase() === o && a++ , r ? n.removeClass("display-none") : n.addClass("display-none"), null }), t.isThereAnExactMatchForItem = !!a, t.appendAddButton(n, o) }) }, e.prototype.handleActionAdd = function (t) { var e = this; if (this.addButton) { var n = $(this.addButton); n.on("click", function () { var o = t.val().toString().toLowerCase().trim(); e.actionPromises.create(o, e.componentOptions).then(function (t) { var i = e.getItem({ id: t, value: o, checked: !1 }), a = { id: t, value: o, checked: !1 }, r = e.items.push(i); n.addClass("saving-item"), e.valuesArray.push(a), e.componentBody.appendChild(i), e.handleItemEvents(i, r), e.addButton.remove() }).catch(function () { return n.removeClass("saving-item") }) }) } }, e.prototype.revertCurrentlyEditedItem = function () { var t = $(this.currentlyEditedItem).find("input"); this.inputInitialValue && t.val(this.inputInitialValue), this.disableItemEditing() }, e.prototype.disableItemEditing = function () { if (this.currentlyEditedItem) { var t = this.currentlyEditedItem.find("input:checkbox"), e = this.currentlyEditedItem.find("input:text"); t.removeAttr("disabled"), e.blur(), document.getSelection().removeAllRanges(), e.attr("readonly", "readonly"), this.currentlyEditedItem.removeClass("editing-item"), this.currentlyEditedItem = null } }, e.prototype.getItem = function (t) { var e = t.checked ? "checked" : "", n = this.actionPromises.update ? '<button class="mr-1" data-role="edit"><span class="material-icons">edit</span></button>\n               <button data-role="accept" class="editing-only mr-1"><span class="material-icons">checked</span></button>\n               <button data-role="undo" class="editing-only"><span class="material-icons">replay</span></button>' : "", o = this.actionPromises.delete ? '<button data-role="delete"><span class="material-icons">clear</span></button>' : "", i = '<div class="item justify-content-start" data-id="' + t.id + '">\n                                   <div class="custom-control custom-checkbox mr-2">\n                                        <input type="checkbox" class="custom-control-input" id="' + t.id + '"\n                                        ' + e + '>\n                                        <label class="custom-control-label d-flex" for="' + t.id + '">\n                                        </label>\n                                   </div>\n\n                                   <input type="text" readonly value="' + t.value + '">\n                                   ' + n + "\n                                   " + o + "\n                                </div>"; return $(i)[0] }, e.prototype.getItems = function () { var t = this, e = []; return this.valuesArray.forEach(function (n) { var o = t.getItem(n); e.push(o) }), e }, e.prototype.appendAddButton = function (t, e) { !this.isThereAnExactMatchForItem && e.trim().length ? this.addButton || (this.addButton = this.getAddButton(), this.componentBody.prepend(this.addButton), this.handleActionAdd(t)) : null !== this.addButton && (this.addButton.remove(), this.addButton = null) }, e.prototype.handleItemEdit = function (t, e, n, o) { var i = this; t.editButton.on("click", function () { i.inputInitialValue = e.val().toString(), i.enableItemEditing(o, e, n) }) }, e.prototype.enableItemEditing = function (t, e, n) { this.revertCurrentlyEditedItem(), this.currentlyEditedItem = t, n.attr("disabled", "disabled"), t.addClass("editing-item"), e.removeAttr("readonly"), e.select() }, e }(d); var f = function () { this.create = null, this.update = null, this.delete = null }; var v = function () { function t(t, e) { this.popoverSettings = e, this.overflowIndicators = [], this.table = $(t), this.handleCellActions(), this.handleTableResize() } return t.prototype.updateIndicatorState = function () { this.resetAllIndicators(), this.handleCellActions() }, t.prototype.handleCellActions = function () { var t = this; this.table.find(".data-cell").each(function (e, n) { t.addOverflowIndicator(n) }), this.enableAllPoppovers() }, t.prototype.addOverflowIndicator = function (t) { var e, n = (e = t).scrollHeight > e.clientHeight || e.scrollWidth > e.clientWidth, o = $(t), i = $(this.getOveflowIndicator(o)); n && 0 === o.children().length && (o.parent().append(i), this.overflowIndicators.push({ cell: o, indicator: i })) }, t.prototype.getOveflowIndicator = function (t) { return '<button class="overflow-indicator btn-primary" data-toggle="popover" data-placement="right"\n                        data-content="' + (t ? t.text() : "") + '" data-trigger="focus">\n                    <div class="material-icons">keyboard_arrow_right</div>\n                </button>' }, t.prototype.handleTableResize = function () { var t = this; $(window).on("resize", function () { var e = t.table.outerWidth(); t.tableWidth = t.tableWidth ? t.tableWidth : e, e !== t.tableWidth && (t.resetAllPoppovers(), t.updateIndicatorState(), t.tableWidth = e) }) }, t.prototype.resetAllIndicators = function () { this.overflowIndicators.forEach(function (t) { t.indicator.remove(), t = null }) }, t.prototype.enableAllPoppovers = function () { this.table.find('[data-toggle="popover"]').popover({ trigger: this.popoverSettings && this.popoverSettings.trigger ? this.popoverSettings.trigger : "focus", template: ' <div class="info-tooltip-content popover popover-dismissible" role="tooltip">\n                            <h3 class="popover-header"></h3>\n                            <div class="popover-body"></div>\n                        </div>' }) }, t.prototype.resetAllPoppovers = function () { this.table.find('[data-toggle="popover"]').popover("dispose") }, t }(); $(document).ready(function () { new o, new r, new s; $.Iso = { Tooltip: a, dynamicFilter: function (t, e, n, o, i, a) { return void 0 === i && (i = { searchBarPlaceholder: "Caută/Adaugă", addButtonLabel: "Adaugă" }), void 0 === a && (a = { placement: "bottom-start" }), o || (o = new f), function (t, e, n, o, i, a) { switch (t) { case "list": var r = new p(n, i, o); return new c(e, r.getComponent(), a); case "multi-select": return r = new m(n, i, o), new c(e, r.getComponent(), a); default: return null } }(t, e, n, o, i, a) }, InlineEditingCells: function () { return new i }, MultilevelCollapse: s, OverflowIndicator: function (t, e) { return new v(t, e) } } }) }]);